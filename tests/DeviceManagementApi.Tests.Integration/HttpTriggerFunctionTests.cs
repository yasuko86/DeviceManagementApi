using DeviceManagementApi.Models;
using DeviceManagementApi.Options;
using DeviceManagementApi.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shouldly;
using System;

namespace DeviceManagementApi.Tests.Integration
{
    public class HttpTriggerFunctionTests : IClassFixture<CosmosContainerFixture>
    {
        private CosmosContainerFixture _cosmosFixture;

        public HttpTriggerFunctionTests(CosmosContainerFixture fixture)
        {
            _cosmosFixture = fixture;
        }

        [Fact]
        public async Task ResgiterDevices_ShouldRegisterAllDevicesInRequest()
        {
            var sampleRequest = ReadSampleRequestFile();
            var sut = GetSut(sampleRequest);

            await sut.Run(sampleRequest);

            // Verify the Cosmos DB content
            var firstDevice = sampleRequest.Devices[0];

            var container = _cosmosFixture.GetContainer();

            var iterator = container.GetItemLinqQueryable<DeviceRecordModel>(true, null, new QueryRequestOptions { MaxItemCount = 1000 }).ToFeedIterator();
            var results = await iterator.ReadNextAsync();
            var registeredDevices = results.ToList();

            registeredDevices.Count.ShouldBe(sampleRequest.Devices.Count);

            var verifyDevice = registeredDevices.Where(x => x.Id == firstDevice.Id).FirstOrDefault();
            verifyDevice!.Name.ShouldBe(firstDevice.Name);
            verifyDevice!.Location.ShouldBe(firstDevice.Location);
            verifyDevice!.Type.ShouldBe(firstDevice.Type);
        }

        private HttpTriggerFunction GetSut(RequestModel request)
        {
            var deviceIds = request.Devices.Select(x => x.Id).ToArray();

            var hostFixture = new DeviceManagementApiFixture();
            var appOptions = hostFixture.Host.Services.GetService<IOptions<AppOptions>>()!;
            var logger = hostFixture.Host.Services.GetService<ILogger<HttpTriggerFunction>>();
            var cosmosClientService = hostFixture.Host.Services.GetService<ICosmosClientService>();

            // Use partially mocked service
            var inventoryService = MockServiceCreator.CreateMockInventoryService(appOptions, deviceIds);

            return new HttpTriggerFunction(logger, inventoryService, cosmosClientService);
        }

        private static RequestModel ReadSampleRequestFile()
        {
            string text = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, TestConstants.DummyResponseTextFile));
            return JsonConvert.DeserializeObject<RequestModel>(text);
        }
    }
}