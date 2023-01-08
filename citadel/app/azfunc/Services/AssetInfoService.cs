using System;
using Citadel.Model.AssetInfo;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Citadel.Services.Data;

namespace Citadel.Services
{
    public class AssetInfoService : IAssetInfoService
    {
        private readonly ILogger<IAssetService> _log;
        private readonly IDatabaseService _dbservice;
        private readonly ICustomerService _customerService;

        public AssetInfoService(ILogger<IAssetService> log, IDatabaseService dbservice, ICustomerService customerService)
        {
            _log = log;
            _dbservice = dbservice;
            _customerService = customerService;
        }

        public List<AssetInfoData> GetAssetInformation(string companyShortName)
        {
            List<AssetInfoData> assetInfoDataList = new List<AssetInfoData>();
            try
            {
                SqlConnection conn = _dbservice.GetSqlConnection();
                conn.Open();

                // Get list of Asset Info Categories for a customer first
                string sqlQuery = @$"SELECT DISTINCT [Category] FROM [AssetToolInfo] WHERE [CompanyShortName] = '{companyShortName}'";
                List<string> categoryList = new List<string>();
                SqlCommand command = new SqlCommand(sqlQuery, conn);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new NoRecordsFoundException("No asset information found for customer");

                    while (reader.Read())
                    {
                        string category = reader.GetString(0);
                        categoryList.Add(category);
                    }
                }
                command.Dispose();

                foreach (string strCategory in categoryList)
                {
                    AssetInfoCategory assetInfoCategory = new AssetInfoCategory();
                    AssetInfoData assetInfoData = new AssetInfoData();
                    assetInfoData.type = Constant.TYPE_ASSET_INFO;
                    assetInfoData.id = Guid.NewGuid().ToString();
                    assetInfoCategory.CategoryName = strCategory;

                    string sqlQuery2 = @$"SELECT ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample FROM AssetToolInfo WHERE CompanyShortName = '{companyShortName}' AND Category = '{strCategory}'";
                    SqlCommand comm2 = new SqlCommand(sqlQuery2, conn);
                    using (SqlDataReader reader2 = comm2.ExecuteReader())
                    {
                        List<Information> informationList = new List<Information>();
                        while (reader2.Read())
                        {
                            Information info = new Information();
                            {
                                info.RequestInfoQuestion = reader2.GetString(0);
                                info.RequestInfoAnswer = reader2.IsDBNull(1) ? null : reader2.GetString(1);
                                info.AdditionalInfo = reader2.IsDBNull(2) ? null : reader2.GetString(2);
                                info.AdditionalInfoExample = reader2.IsDBNull(3) ? null : reader2.GetString(3);
                            };
                            informationList.Add(info);
                            assetInfoCategory.informationList = informationList;
                        }
                        assetInfoCategory.informationList = informationList;
                    }
                    assetInfoData.attributes = assetInfoCategory;
                    assetInfoDataList.Add(assetInfoData);
                }
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
            return assetInfoDataList;
        }

        public void UpdateAssetInformation(List<AssetInfoData> dataMessage, string companyShortName)
        {
            try
            {
                DeleteAssetInformationForCompany(companyShortName);
            }
            catch
            {

            }
            finally
            {
                AddNewAssetInformation(dataMessage, companyShortName);
            }

        }

        private void AddNewAssetInformation(List<AssetInfoData> dataMessage, string companyShortName)
        {
            try
            {
                string baseSqlQuery = "INSERT INTO AssetToolInfo (CompanyShortName, Category, ReqInfoQuestion, ReqInfoAnswer, AdditionalInfo, AdditionalInfoExample) VALUES ";
                string sqlQuery = "";

                // this will add a new customer if the customer doesn't already exist.  If customer exists, then nothing will get added
                _customerService.AddCustomer(companyShortName);

                SqlConnection conn = _dbservice.GetSqlConnection();
                conn.Open();

                foreach (AssetInfoData assetInfoData in dataMessage)
                {
                    AssetInfoCategory assetInfoCategory = assetInfoData.attributes;
                    string strCategory = assetInfoCategory.CategoryName;
                    foreach (Information info in assetInfoCategory.informationList)
                    {
                        sqlQuery = baseSqlQuery + $"('{companyShortName}', '{strCategory}', '{PrepForSqlInsert(info.RequestInfoQuestion)}', '{PrepForSqlInsert(info.RequestInfoAnswer)}', '{PrepForSqlInsert(info.AdditionalInfo)}', '{PrepForSqlInsert(info.AdditionalInfoExample)}')";
                        SqlCommand comm = new SqlCommand(sqlQuery, conn);
                        comm.ExecuteNonQuery();
                        comm.Dispose();
                    }
                }
                conn.Close();
            }
            catch (BadRequestException e)
            {
                _log.LogError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
                throw;
            }
        }

        public void DeleteAssetInformationForCompany(string companyShortName)
        {
            try
            {
                string sqlQuery = $"DELETE from AssetToolInfo WHERE CompanyShortName = '{companyShortName}'";
                SqlConnection conn = _dbservice.GetSqlConnection();
                conn.Open();
                SqlCommand comm = new SqlCommand(sqlQuery, conn);
                int rowsAffected = comm.ExecuteNonQuery();
                comm.Dispose();
                conn.Close();
                if (rowsAffected == 0)
                    throw new NoRecordsFoundException($"Cannot delete asset information for customer '{companyShortName}'.  No asset information not found for company.");
            }
            catch (NoRecordsFoundException e)
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

        private string PrepForSqlInsert(string value)
        {
            return value.Replace("'", "''");
        }
    }
}
