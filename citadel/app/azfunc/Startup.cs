using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Citadel.Services;
using Citadel.Services.Data;

[assembly: FunctionsStartup(typeof(Citadel.Startup))]
namespace Citadel
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IDatabaseService, DatabaseService>();
            builder.Services.AddTransient<ICustomerService, CustomerService>();
            builder.Services.AddTransient<IAssetService, AssetService>();
            builder.Services.AddTransient<IAssetInfoService, AssetInfoService>();
            builder.Services.AddTransient<IAssetParseService, AssetParseService>();
            builder.Services.AddLogging();
        }
    }
}