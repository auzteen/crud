using Citadel.Model.AssetInfo;
using System.Collections.Generic;

namespace Citadel.Services
{
    public interface IAssetInfoService
    {
        public List<AssetInfoData> GetAssetInformation(string companyShortName);

        void UpdateAssetInformation(List<AssetInfoData> dataMessage, string companyShortName);

        void DeleteAssetInformationForCompany(string companyShortName);
    }
}