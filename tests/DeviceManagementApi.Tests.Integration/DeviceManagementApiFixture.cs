using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace DeviceManagementApi.Tests.Integration
{
    public class DeviceManagementApiFixture
    {
        public IHost Host { get; }

        public DeviceManagementApiFixture()
        {
            SetupEnvironment();

            var startup = new Startup();
            Host = new HostBuilder()
                .ConfigureWebJobs((context, builder) => new Startup().Configure(new WebJobsBuilderContext
                {
                    ApplicationRootPath = context.HostingEnvironment.ContentRootPath,
                    Configuration = context.Configuration,
                    EnvironmentName = context.HostingEnvironment.EnvironmentName,
                }, builder))
                .Build();
        }

        private void SetupEnvironment()
        {
            var settings = JsonConvert.DeserializeObject<TestSettings>(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, TestConstants.TestSettingsFile)));

            foreach (var setting in settings.Values)
            {
                Environment.SetEnvironmentVariable(setting.Key, setting.Value);
            }
        }
    }
}
