using System;
using Citadel.Model.Asset;
using Citadel.Model.Qualys;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Citadel.Services.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Dynamic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Hosting;
using System.Reflection.Metadata;
using Citadel.Model.Defender;

namespace Citadel.Services
{
    public class AssetService : IAssetService
    {
        private readonly ILogger<IAssetService> _log;
        private readonly IDatabaseService _dbservice;
        private readonly ICustomerService _customerService;

        public AssetService(ILogger<IAssetService> log, IDatabaseService dbservice, ICustomerService customerService)
        {
            _log = log;
            _dbservice = dbservice;
            _customerService = customerService;
        }

        public List<AssetData> GetCustomerAssets(string companyShortName, string pageLimit, string pageOffset)
        {
            List<AssetData> assetDataList = new List<AssetData>();
            this.GetPageMetaData(companyShortName, Int32.Parse(pageLimit), out int totalRecords, out int totalPageCount);

            try
            {
                SqlConnection conn = _dbservice.GetSqlConnection();
                conn.Open();

                SqlCommand comm = new SqlCommand("spGetOffsetPageAssets", conn);
                comm.CommandType = CommandType.StoredProcedure;
                SqlParameter paramPageLimit = new SqlParameter("@PageLimit", SqlDbType.Int);
                paramPageLimit.Value = Int32.Parse(pageLimit);
                comm.Parameters.Add(paramPageLimit);

                SqlParameter paramPageOffset = new SqlParameter("@PageOffset", SqlDbType.Int);
                paramPageOffset.Value = Int32.Parse(pageOffset);
                comm.Parameters.Add(paramPageOffset);

                SqlParameter paramCompanyShort = new SqlParameter("@CompanyShortName", SqlDbType.NVarChar, 15);
                paramCompanyShort.Value = companyShortName;
                comm.Parameters.Add(paramCompanyShort);

                string strAssetType = "";
                Assets assets = new Assets();
                List<Asset> assetList = new List<Asset>();
                bool emptyAsset = true;
                AssetData assetData;
                using (SqlDataReader reader = comm.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new NoRecordsFoundException("No assets found for customer");
                    else
                    {
                        assetData = CreateNewAssetDataObject();
                        assetData.PageCount = totalPageCount;
                        assetData.TotalRecords = totalRecords;
                    }
                    while (reader.Read())
                    {
                        if (strAssetType != reader.GetString(2))
                        {
                            if (!emptyAsset)
                            {
                                assetData.attributes = assets;
                                assetData.PageCount = totalPageCount;
                                assetData.TotalRecords = totalRecords;
                                assetDataList.Add(assetData);
                                assetData = CreateNewAssetDataObject();
                            }
                            assets = new Assets();
                            strAssetType = reader.GetString(2);
                            assets.assetType = strAssetType;
                            assetList = new List<Asset>();
                        }

                        AssetCriticality ac = new AssetCriticality();
                        ac.status = reader.IsDBNull(11) ? null : reader.GetString(11);
                        ac.score = reader.IsDBNull(12) ? null : reader.GetInt32(12);

                        Asset asset = new Asset()
                        {
                            AssetId = reader.GetInt32(0),
                            AssetName = reader.GetString(1),
                            AssetTier = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Domain = reader.IsDBNull(5) ? null : reader.GetString(5),
                            IPAddress = reader.IsDBNull(6) ? null : reader.GetString(6),
                            IPLocation = reader.IsDBNull(7) ? null : reader.GetString(7),
                            Manufacturer = reader.IsDBNull(8) ? null : reader.GetString(8),
                            OS = reader.IsDBNull(9) ? null : reader.GetString(9),
                            Comment = reader.IsDBNull(10) ? null : reader.GetString(10),
                            assetCriticality = ac
                        };

                        assetList.Add(asset);
                        assets.assetList = assetList;
                        emptyAsset = false;

                    }
                    assetData.attributes = assets;
                    assetDataList.Add(assetData);
                }
                //assetDataList.Add(assetData);
                conn.Close();

            }
            catch (NoRecordsFoundException e)
            {
                _log.LogInformation(e.Message + $" -- customer {companyShortName}");
                throw;
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
                throw;
            }
            return assetDataList;
        }

        private AssetData CreateNewAssetDataObject()
        {
            AssetData assetData = new AssetData();
            assetData.type = Constant.TYPE_ASSETS;
            assetData.id = Guid.NewGuid().ToString();
            return assetData;
        }

        private void GetPageMetaData(string companyShortName, int pageSize, out int totalRecords, out int totalPageCount)
        {
            string sqlQuery = $"SELECT COUNT(*) FROM vwAssets WHERE CompanyShortName = '{companyShortName}'";
            totalRecords = _dbservice.GetScalarValueFromQuery(sqlQuery);
            if (pageSize == 0)
                totalPageCount = 1;
            if ((totalRecords % pageSize) == 0)
                totalPageCount = (totalRecords / pageSize);
            else
                totalPageCount = ((totalRecords / pageSize) + 1);
        }

        private int GetNextPageCount(string companyShortName, string pageSize, string cursorVal)
        {
            return GetCursorPageCount(companyShortName, pageSize, cursorVal, "dbo.spGetNextPageAssetCount");
        }

        private int GetPrevPageCount(string companyShortName, string pageSize, string cursorVal)
        {
            return GetCursorPageCount(companyShortName, pageSize, cursorVal, "dbo.spGetPrevPageAssetCount");
        }

        private int GetCursorPageCount(string companyShortName, string pageSize, string cursorVal, string spName)
        {
            SqlConnection conn = _dbservice.GetSqlConnection();
            conn.Open();
            SqlCommand cmd = new SqlCommand(spName, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("Limit", pageSize);
            cmd.Parameters.AddWithValue("CursorVal", cursorVal);
            cmd.Parameters.AddWithValue("CompanyShortName", companyShortName);
            var returnVal = cmd.Parameters.Add("@returnVal", SqlDbType.Int);
            returnVal.Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();
            var result = returnVal.Value;
            conn.Close();
            return (int)result;
        }

        public ObjectResult AddCustomerAssets(List<AssetData> dataMessage, string companyShortName)
        {

            int assetCount = 0;
            int missedAssetCount = 0;
            //AssetAttribute assetData = dataMessage.data;
            //List<Assets> assetAttributes = assetData.attributes;
            //string companyShortName = assetData.id;
            try
            {
                // this will add a new customer if the customer doesn't already exist.  If customer exists, then nothing will get added
                _customerService.AddCustomer(companyShortName);

                string baseSqlQuery = "INSERT INTO Asset (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment, CriticalityStatus, CriticalityScore) VALUES ";
                string sqlQuery = "";

                SqlConnection conn = _dbservice.GetSqlConnection();
                conn.Open();

                foreach (AssetData data in dataMessage)
                {
                    string strAssetType = data.attributes.assetType;

                    foreach (Asset asset in data.attributes.assetList)
                    {
                        try
                        {
                            if (String.IsNullOrEmpty(strAssetType))
                                throw new BadRequestException("Bad request format - missing asset type");

                            if (String.IsNullOrEmpty(asset.AssetName))
                                throw new BadRequestException("Bad request format - missing asset name");

                            assetCount = assetCount + 1;

                            // Asset Criticality does not exist when adding assets from csv
                            string criticalityStatus = string.Empty;
                            int? criticalityScore = null;

                            if (asset.assetCriticality != null)
                            {
                                criticalityStatus = asset.assetCriticality.status;
                                criticalityScore = asset.assetCriticality.score;
                            }

                            sqlQuery = baseSqlQuery + $"('{asset.AssetName}', '{strAssetType}', '{companyShortName}', '{asset.AssetTier}', '{asset.Domain}', '{asset.IPAddress}', '{asset.IPLocation}', '{asset.Manufacturer}', '{asset.OS}', '{asset.Comment}', '{criticalityStatus}', '{criticalityScore}')";
                            SqlCommand comm = new SqlCommand(sqlQuery, conn);
                            comm.ExecuteNonQuery();
                            comm.Dispose();
                        }
                        catch (SqlException ex)
                        {
                            missedAssetCount = missedAssetCount + 1;
                            string message = $"Asset Name: '{asset.AssetName}' -- Asset Type: '{strAssetType}'";
                            _log.LogWarning("** Error ** Cannot add asset that already exists for customer. ** Updating instead");
                            _log.LogWarning(message);
                            _log.LogError(ex.Message);
                        }
                        catch (Exception e)
                        {
                            string message = e.Message.Length > 0 ? e.Message : "Bad request format";
                            _log.LogInformation(message);
                            throw new BadRequestException(message);
                        }
                    }
                }
                conn.Close();
                string statusMessage = "";

                if ((assetCount == missedAssetCount) && (assetCount > 0))
                {
                    statusMessage = "The list of supplied assets already exist for the customer.";
                    throw new ExistingAssetsException(statusMessage);
                }
                else if (missedAssetCount > 0)
                {
                    return new OkObjectResult("Some assets could not be added as they already exist for the customer")
                    {
                        StatusCode = StatusCodes.Status201Created
                    };
                }
                else
                {
                    return new OkObjectResult("Request OK. Nothing To Update")
                    {
                        StatusCode = StatusCodes.Status200OK
                    };
                }
            }
            catch (ExistingAssetsException e)
            {
                _log.LogInformation(e.Message);
                throw;
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
                throw;
            }
        }

        public List<ExpandoObject> PutCustomerAssets(List<AssetData> dataMessage, string companyShortName)
        {
            List<ExpandoObject> responseList = new List<ExpandoObject>();
            try
            {
                // Response
                dynamic response = null;

                // Add customer if not exists customer
                _customerService.AddCustomer(companyShortName);
                foreach (AssetData data in dataMessage)
                {
                    // Dynamic Object
                    response = new ExpandoObject();

                    // Counts
                    response.totalAssetCount = 0;
                    response.addedAssetCount = 0;
                    response.updatedAssetCount = 0;

                    // Asset Types
                    response.assetType = data.attributes.assetType;

                    // Assets (No Added..)
                    response.updatedAssets = new List<dynamic>();

                    // Assets
                    foreach (Asset asset in data.attributes.assetList)
                    {
                        try
                        {
                            if (String.IsNullOrEmpty(data.attributes.assetType))
                                throw new BadRequestException("Bad request format - missing asset type");

                            if (String.IsNullOrEmpty(asset.AssetName))
                                throw new BadRequestException("Bad request format - missing asset name");

                            if (!AssetExists(asset, companyShortName, data.attributes.assetType, out int assetId))
                            {
                                AddAsset(asset, companyShortName, data.attributes.assetType);
                                if (!AssetExists(asset, companyShortName, data.attributes.assetType, out assetId))
                                    throw new FailedToUpdateAssetException("Failed to create asset");
                                else
                                {
                                    response.totalAssetCount++;
                                    response.addedAssetCount++;
                                }
                            }
                            else
                            {
                                string assetUpdateResponse = string.Empty;
                                if (NeedToUpdate(assetId.ToString(), asset, data.attributes.assetType, out dynamic fields))
                                {
                                    if (UpdateAsset(asset, companyShortName, data.attributes.assetType, assetId))
                                    {
                                        var updateList = new List<dynamic>();

                                        foreach (var field in fields)
                                        {
                                            updateList.Add($"{field.Key.ToString()}: {field.Value.ToString()}");
                                        }

                                        dynamic[] updateArray = updateList.ToArray();
                                        var updatedAssetDictionary = new Dictionary<int, dynamic>();
                                        updatedAssetDictionary.Add(assetId, updateArray);
                                        response.updatedAssets.Add(updatedAssetDictionary);
                                        response.totalAssetCount++;
                                        response.updatedAssetCount++;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _log.LogError(e.Message);
                            throw;
                        }
                    }

                    // No Counts for this asset
                    if (response.addedAssetCount != 0 || response.updatedAssetCount != 0)
                    {
                        IDictionary<string, object> map = response;
                        if (response.addedAssetCount == 0)
                            map.Remove("addedAssetCount");

                        if (response.updatedAssetCount == 0)
                            map.Remove("updatedAssetCount");

                        if (response.totalAssetCount == 0)
                            map.Remove("totalAssetCount");

                        responseList.Add(response);
                    }
                }
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
                throw;
            }

            return responseList;

        }

        private bool AssetExists(Asset asset, string companyShortName, string assetType, out int assetId)
        {
            SqlConnection conn = _dbservice.GetSqlConnection();
            string query = $"SELECT Top(1) AssetID from Asset WHERE AssetName = '{asset.AssetName}' and AssetType = '{assetType}' and CompanyShortName = '{companyShortName}'";

            try
            {
                int ord = -1;
                int asset_id = -1;

                conn.Open();
                SqlCommand comm = new SqlCommand(query, conn);
                var reader = comm.ExecuteReader();

                if (reader.Read())
                {
                    ord = reader.GetOrdinal("AssetId");
                    asset_id = reader.GetInt32(ord);

                    if (asset_id > 0)
                    {
                        assetId = asset_id;
                        comm.Dispose();
                        return true;
                    }
                    else
                    {
                        assetId = asset_id;
                        comm.Dispose();
                        return false;
                    }
                }
                else
                {
                    assetId = asset_id;
                    comm.Dispose();
                    return false;
                }
            }
            catch (SqlException e)
            {
                _log.LogError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
                throw;
            }
            finally
            {
                conn.Dispose();
            }

        }

        private bool AddAsset(Asset asset, string companyShortName, string assetType)
        {
            // Sql
            string baseSqlQuery = "INSERT INTO Asset (AssetName, AssetType, CompanyShortName, AssetTier, Domain, IPAddress, IPLocation, Manufacturer, OS, Comment, CriticalityStatus, CriticalityScore) VALUES ";
            string sqlQuery = "";
            SqlConnection conn = _dbservice.GetSqlConnection();

            // Criticality
            string criticalityStatus = string.Empty;
            int? criticalityScore = null;

            try
            {
                conn.Open();

                if (asset.assetCriticality != null)
                {
                    criticalityStatus = asset.assetCriticality.status;
                    criticalityScore = asset.assetCriticality.score;
                }

                sqlQuery = baseSqlQuery + $"('{asset.AssetName}', '{assetType}', '{companyShortName}', '{asset.AssetTier}', '{asset.Domain}', '{asset.IPAddress}', '{asset.IPLocation}', '{asset.Manufacturer}', '{asset.OS}', '{asset.Comment}', '{criticalityStatus}', '{criticalityScore}')";
                SqlCommand comm = new SqlCommand(sqlQuery, conn);
                comm.ExecuteNonQuery();
                comm.Dispose();

                return true;

            }
            catch (SqlException e)
            {
                _log.LogError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
                throw;
            }
            finally
            {
                conn.Dispose();
            }
        }

        public bool UpdateAsset(Asset asset, string companyShortName, string assetType, int assetId)
        {
            // Sql
            SqlConnection conn = _dbservice.GetSqlConnection();
            conn.Open();
            string query = "UPDATE Asset Set ";

            // Asset Type
            query += $" AssetType='{assetType}',";

            // Company Short Name
            query += $" CompanyShortName='{companyShortName}',";

            try
            {
                var assetSerialized = JsonConvert.SerializeObject(asset);
                JObject keyValuePairs = JObject.Parse(assetSerialized.ToString());
                foreach (var pair in keyValuePairs)
                {
                    // Asset Id
                    if (pair.Key != "AssetId")
                    {
                        if (pair.Key != "assetCriticality")
                        {
                            query += $" {pair.Key.ToString()}='{pair.Value.ToString()}',";
                        }
                        else if (pair.Key.ToString() == "assetCriticality" && pair.Value.ToString() != "")
                        {
                            JObject kvPairs = JObject.Parse(pair.Value.ToString());
                            foreach (var pr in kvPairs)
                            {
                                if (pr.Key.ToString() == "status")
                                {
                                    query += $" CriticalityStatus='{pr.Value.ToString()}',";
                                }
                                else
                                {
                                    var val = int.TryParse(pr.Value.ToString(), out var intVal) ? (int?)intVal : null;
                                    if (val == null)
                                    {
                                        query += $" CriticalityScore='{null}',";
                                    }
                                    else if (val >= 0 || val <= 10)
                                    {
                                        query += $" CriticalityScore='{pr.Value.ToString()}',";
                                    }
                                    else
                                    {
                                        throw new BadRequestException("Criticality score must be a valid number 0 through 10 inclusive!");
                                    }
                                }
                            }
                        }
                    }
                }
                query = query.TrimEnd(',');
                query += $" WHERE AssetId = '{assetId}'";

                // Setup
                SqlCommand comm = new SqlCommand(query, conn);

                // Execute
                comm.ExecuteNonQuery();
                conn.Close();

                return true;

            }
            catch (SqlException e)
            {
                _log.LogError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
                throw;
            }
            finally
            {
                conn.Dispose();
            }
        }

        public bool NeedToUpdate(string asset_id, Asset asset, string assetType, out dynamic fields)
        {
            // Flag
            bool needToUpdate = false;

            // Fields
            ExpandoObject fieldList = new ExpandoObject();

            // Connection
            SqlConnection conn = _dbservice.GetSqlConnection();
            conn.Open();

            // Query
            string sqlQuery = $"SELECT TOP(1) * FROM Asset WHERE AssetId = {asset_id} ";

            // Command
            SqlCommand comm = new SqlCommand(sqlQuery, conn);

            try
            {
                // Execute
                SqlDataReader reader = comm.ExecuteReader();

                // Read
                if (reader.Read())
                {
                    var assetSerialized = JsonConvert.SerializeObject(asset);
                    JObject assetKeyValuePairs = JObject.Parse(assetSerialized);
                    foreach (var pair in assetKeyValuePairs)
                    {
                        int ord;

                        // Ignore
                        if (pair.Key != "AssetId")
                        {
                            ord = reader.GetOrdinal("AssetType");
                            if (reader.GetString(ord) != assetType)
                            {
                                fieldList.TryAdd(pair.Key.ToString(), pair.Value.ToString());
                                needToUpdate = true;
                            }

                            if (pair.Key != "assetCriticality")
                            {
                                ord = reader.GetOrdinal(pair.Key);
                                if (reader.GetString(ord) != pair.Value.ToString())
                                {
                                    fieldList.TryAdd(pair.Key.ToString(), pair.Value.ToString());
                                    needToUpdate = true;
                                }
                            }
                            else
                            {
                                if (pair.Value.ToString() != string.Empty)
                                {
                                    JObject keyValuePairs = JObject.Parse(pair.Value.ToString());
                                    foreach (var pr in keyValuePairs)
                                    {
                                        if (pr.Key.ToString() == "status")
                                        {
                                            ord = reader.GetOrdinal("CriticalityStatus");
                                            if ((reader.IsDBNull(ord) && pr.Value != null) || (reader.GetString(ord) != pr.Value.ToString()))
                                            {
                                                fieldList.TryAdd(pr.Key.ToString(), pr.Value.ToString());
                                                needToUpdate = true;
                                            }
                                        }
                                        else
                                        {
                                            if (!int.TryParse(pr.Value.ToString(), out int val1))
                                            {
                                                ord = reader.GetOrdinal("CriticalityScore");
                                                if ((reader.IsDBNull(ord) && pr.Value != null) || (reader.GetInt32(ord) != val1))
                                                {
                                                    fieldList.TryAdd(pr.Key.ToString(), pr.Value.ToString());
                                                    needToUpdate = true;
                                                }
                                            }
                                            else if (int.TryParse(pr.Value.ToString(), out int val2) && (val2 >= 0 && val2 <= 10))
                                            {
                                                ord = reader.GetOrdinal("CriticalityScore");
                                                if ((reader.IsDBNull(ord) && pr.Value != null) || (reader.GetInt32(ord) != val1))
                                                {
                                                    fieldList.TryAdd(pr.Key.ToString(), pr.Value.ToString());
                                                    needToUpdate = true;
                                                }
                                            }
                                            else
                                            {
                                                throw new BadRequestException($"Criticality score, {pr.Value}, invalid.  Criticality score must be a valid number 0 through 10 inclusive!");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                _log.LogError(e.Message);
                throw;
            }
            catch (KeyNotFoundException e)
            {
                _log.LogError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
                throw;
            }
            finally
            {
                conn.Close();
            }
            fields = fieldList;
            return needToUpdate;
        }

        public List<AssetData> MapQualysToAssetData(QualysData data)
        {
            AssetData assetData = new AssetData();
            assetData.type = Constant.TYPE_ASSETS;
            assetData.id = null;
            Assets assets = new Assets();
            assets.assetType = "Qualys Assets";
            List<Asset> assetList = new List<Asset>();

            List<Host> hostList = data.response.hostList.host;
            foreach (Host host in hostList)
            {
                Asset newAsset = new Asset();
                AssetCriticality ac = new AssetCriticality();
                newAsset.IPAddress = host.ip;
                newAsset.Domain = host?.dnsData?.domain == null ? null : host.dnsData.domain;
                newAsset.AssetName = host?.dnsData?.hostname == null ? $"asset_name: {host.ip}" : host.dnsData.hostname;
                newAsset.Comment = $"Qualys ID: {host.id}";
                newAsset.OS = host?.os == null ? null : host.os;

                // null values
                newAsset.AssetTier = null;
                newAsset.IPLocation = null;
                newAsset.Manufacturer = null;
                ac.status = "";
                ac.score = 0;
                newAsset.assetCriticality = ac;

                assetList.Add(newAsset);
            }
            assets.assetList = assetList;
            assetData.attributes = assets;
            List<AssetData> returnData = new List<AssetData>();
            returnData.Add(assetData);
            return returnData;
        }

        /// <summary>
        /// Function to convert Response Data recieved from Macines API to Defender Data
        /// </summary>
        /// <param name="responseData"></param>
        /// <returns></returns>
        public RootData ConvertToAssetData(JsonResponseData responseData)
        {
            RootData rootData = new RootData();
            if (responseData != null)
            {
                List<DefenderData> lst = new List<DefenderData>();
                List<DefenderAsset> assets = new List<DefenderAsset>();

                foreach (var data in responseData.Value)
                {
                    assets.Add(new DefenderAsset
                    {
                        AssetName = data.computerDnsName,
                        AssetTier = "",
                        Domain = "",
                        IpAddress = data.lastIpAddress,
                        IpLocation = "",
                        Manufacturer = "",
                        OS = data.osPlatform,
                        Comment = "",
                        SourceId = data.SourceId,
                        AssetCriticality = new DefenderCriticality
                        {
                            Status = data.exposureLevel,
                            Score = 0
                        }
                    });
                }
                var assetData = new DefenderData
                {
                    AssetType = "assets",
                    Id = "8509ea72-6292-4374-ac92-783d91ec498b",
                    AssetAttributes = new DefenderAttributes
                    {
                        AssetType = "Asset/Scan Servers",
                        Assets = assets.ToArray()
                    }
                };

                lst.Add(assetData);
                rootData.DefenderData = new DefenderData[] { assetData };
            }
            return rootData;
        }
    }
}
