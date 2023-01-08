namespace Citadel
{
    public class Constant
    {
        // Pagination
        public const string DEFAULT_ASSET_PAGE_SIZE = "10";
        public const string DEFAULT_ASSET_PAGE_OFFSET = "0";

        //Version
        public const string VERSION_PATH = "v1";

        // Function Names
        public const string GET_CUSTOMER = "GetCustomers";
        public const string GET_ASSET = "GetCustomerAssets";
        public const string PUT_ASSET = "PutCustomerAssets";
        public const string GET_ASSET_INFO = "GetAssetInfo";
        public const string POST_ASSET_INFO = "PostAssetInfo";
        public const string UPDATE_ASSET = "UpdateCustomerAssets";
        public const string PARSE_ASSET = "ParseCustomerAssets";


        // Routes
        public const string CUSTOMERS_ROUTE = VERSION_PATH + "/" + TYPE_CUSTOMERS + "/{id?}";
        public const string ASSETS_ROUTE = VERSION_PATH + "/" + TYPE_CUSTOMERS + "/{id?}/" + TYPE_ASSETS + "/";
        public const string ASSETS_PUT_ROUTE = VERSION_PATH + "/" + TYPE_CUSTOMERS + "/{id?}/" + TYPE_ASSETS + "/";
        public const string ASSETS_INFO_ROUTE = VERSION_PATH + "/" + TYPE_CUSTOMERS + "/{id?}/" + TYPE_ASSET_INFO + "/";
        public const string ASSETS_INFO_POST_ROUTE = VERSION_PATH + "/" + TYPE_CUSTOMERS + "/{id?}/" + TYPE_ASSET_INFO + "/";
        public const string ASSETS_UPDATE_ROUTE = VERSION_PATH + "/" + TYPE_CUSTOMERS + "/{id?}/" + TYPE_ASSETS + "/{asset_id?}";
        public const string GET_DEFENDER_HOSTS_ROUTE = VERSION_PATH + "/" + TYPE_DEFENDER + "/";
        public const string GET_QUALYS_HOSTS_ROUTE = VERSION_PATH + "/" + TYPE_CUSTOMERS + "/{id?}/" + TYPE_QUALYS + "/";
        public const string ASSETS_PARSE_ROUTE = VERSION_PATH + "/" + TYPE_CUSTOMERS + "/{id?}/" + TYPE_ASSETS + "/parse/";


        // Types
        public const string TYPE_CUSTOMERS = "customers";
        public const string TYPE_ASSETS = "assets";
        public const string TYPE_ASSET_INFO = "asset-info";
        public const string TYPE_DEFENDER = "defender";
        public const string TYPE_QUALYS = "qualys-assets";
    }
}