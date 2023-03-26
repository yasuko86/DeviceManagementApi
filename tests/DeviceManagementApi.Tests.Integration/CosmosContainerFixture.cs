using DeviceManagementApi.Options;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace DeviceManagementApi.Tests.Integration
{
    public class CosmosContainerFixture : IAsyncLifetime
    {
        private Database _db;
        private CosmosDbOptions _dbOptions;

        public CosmosContainerFixture()
        {
            _dbOptions = GetDbOptions();
        }

        public async Task InitializeAsync()
        {
            _db = await new CosmosClient(_dbOptions.Uri, _dbOptions.Key).CreateDatabaseIfNotExistsAsync(_dbOptions.DatabaseName);
        }

        public async Task DisposeAsync()
        {
            await _db.DeleteAsync();
        }

        public Container GetContainer()
        {
            return _db.GetContainer(_dbOptions.ContainerName);
        }

        private CosmosDbOptions GetDbOptions()
        {
            var settings = JsonConvert.DeserializeObject<TestSettings>(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, TestConstants.TestSettingsFile)));

            return new CosmosDbOptions
            {
                Uri = settings.Values["CosmosDbOptions__Uri"],
                Key = settings.Values["CosmosDbOptions__Key"],
                DatabaseName = settings.Values["CosmosDbOptions__DatabaseName"],
                ContainerName = settings.Values["CosmosDbOptions__ContainerName"],
            };
        }
    }
}
