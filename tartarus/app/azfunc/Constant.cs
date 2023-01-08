namespace Tartarus
{
    public class Constant
    {
        //Version
        public const string VERSION_PATH = "v1";

        // Function Names
        public const string POST_ASSET_VULNERABILITY = "PostAssetVolunerability";
        public const string POST_ASSET_MITIGATION = "PostAssetMitigation";
        public const string POST_ASSET_MITIGATION_ARCHIVE = "PostAssetMitigationArchive";

        // Routes
        public const string ASSETS_VOLUNERABILITY_ROUTE = VERSION_PATH + "/" + TYPE_CUSTOMERS + "/{id?}/" + TYPE_VULNERABILITY + "/";
        public const string ASSETS_MITIGATION_ROUTE = VERSION_PATH + "/" + TYPE_CUSTOMERS + "/{id?}/" + TYPE_MITIGATION + "/";
        public const string ASSETS_MITIGATION_ARCHIVE_ROUTE = VERSION_PATH + "/" + TYPE_CUSTOMERS + "/{id?}/" + TYPE_MITIGATION + "/archive";

        // Types
        public const string TYPE_CUSTOMERS = "customers";
        public const string TYPE_VULNERABILITY = "vulnerability";
        public const string TYPE_MITIGATION = "mitigation";
    }
}