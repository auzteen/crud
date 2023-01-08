using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Tartarus.Services;
using Tartarus.Services.Data;

[assembly: FunctionsStartup(typeof(Tartarus.Startup))]
namespace Tartarus
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IDatabaseService, DatabaseService>();
            builder.Services.AddTransient<IVulnerabilityService, VulnerabilityService>();
            builder.Services.AddLogging();
        }
    }
}