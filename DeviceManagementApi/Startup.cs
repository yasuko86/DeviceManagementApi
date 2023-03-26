using DeviceManagementApi.Options;
using DeviceManagementApi.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(DeviceManagementApi.Startup))]

namespace DeviceManagementApi
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(builder.GetContext().ApplicationRootPath)
                .AddJsonFile("local.settings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddOptions<AppOptions>()
                    .Configure<IConfiguration>((settings, configuration) =>
                    {
                        configuration.Bind(settings);
                    });

            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IInventoryService, InventoryService>();
            builder.Services.AddSingleton<ICosmosClientService, CosmosClientService>();
        }
    }
}
