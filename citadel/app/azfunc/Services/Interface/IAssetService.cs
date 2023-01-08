using Citadel.Model.Asset;
using Citadel.Model.Qualys;
using Citadel.Model.Defender;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Dynamic;

namespace Citadel.Services
{
    public interface IAssetService
    {
        List<AssetData> GetCustomerAssets(string companyShortName, string pageLimit, string pageOffset);
        ObjectResult AddCustomerAssets(List<AssetData> dataMessage, string companyShortName);
        public bool NeedToUpdate(string asset_id, Asset asset, string assetType, out dynamic fields);
        public List<ExpandoObject> PutCustomerAssets(List<AssetData> dataMessage, string companyShortName);
        public bool UpdateAsset(Asset asset, string companyShortName, string assetType, int assetId);
        List<AssetData> MapQualysToAssetData(QualysData data);
        public RootData ConvertToAssetData(JsonResponseData responseData);
    }
}