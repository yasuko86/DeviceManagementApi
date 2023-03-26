using DeviceManagementApi.Models;
using DeviceManagementApi.Options;
using DeviceManagementApi.Services;
using MicrosoftExtOptions = Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net.Http.Json;
using Shouldly;

namespace DeviceManagementApi.Tests
{
    public class InventoryServiceTests
    {
        private readonly MicrosoftExtOptions.IOptions<AppOptions> appOptions = CreateAppOptions();

        [Fact]
        public async Task GetAsync_HappyPath()
        {
            // Arrange
            var deviceId = "DVID000001";
            var reponseContentModel = CreateDeviceResponseModel(deviceId);

            var mockResponse = CreateSuccessResponse<InventoryDeviceModel>(reponseContentModel);

            var mockHttpClientFactory = CreatMockHttpClientFactory(mockResponse);

            var sut = new InventoryService(mockHttpClientFactory.Object, appOptions);

            // Act
            var result = await sut.GetAsync(deviceId);

            // Assert
            result.ShouldBeOfType<InventoryDeviceModel>();
            result.DeviceId.ShouldBe(deviceId);
        }

        [Fact]
        public async Task GetAsync_UnhappyPath()
        {
            // Arrange
            var deviceId = "DVID000001";

            var mockResponse = CreateErrorResponse();

            var mockHttpClientFactory = CreatMockHttpClientFactory(mockResponse);

            var sut = new InventoryService(mockHttpClientFactory.Object, appOptions);

            // Act & Assert
            await Should.ThrowAsync<Exception>(async() => await sut.GetAsync(deviceId));
        }


        [Fact]
        public async Task PostAsync_HappyPath()
        {
            // Arrange
            var deviceIds = new string[] { "DVID000001", "DVID000002", "DVID000003" };
            var reponseContentModel = CreateDeviceListResponseModel(deviceIds);

            var mockResponse = CreateSuccessResponse<InventoryDeviceListModel>(reponseContentModel);

            var mockHttpClientFactory = CreatMockHttpClientFactory(mockResponse);

            var sut = new InventoryService(mockHttpClientFactory.Object, appOptions);

            // Act
            var result = await sut.PostAsync(deviceIds);

            // Assert
            result.ShouldBeOfType<InventoryDeviceListModel>();
            result.Devices.Count.ShouldBe(3);
            result.Devices.ForEach(x => deviceIds.ShouldContain(x.DeviceId));
        }

        [Fact]
        public async Task PostAsync_UnhappyPath()
        {
            // Arrange
            var deviceIds = new string[] { "DVID000001", "DVID000002", "DVID000003" };

            var mockResponse = CreateErrorResponse();

            var mockHttpClientFactory = CreatMockHttpClientFactory(mockResponse);

            var sut = new InventoryService(mockHttpClientFactory.Object, appOptions);

            // Act & Assert
            await Should.ThrowAsync<Exception>(async() => await sut.PostAsync(deviceIds));
        }

        private static MicrosoftExtOptions.IOptions<AppOptions> CreateAppOptions()
        {
            var cosmosOptions = new CosmosDbOptions
            {
                Uri = "https://testcosmos:8081",
                Key = "cosmos-test-key",
                DatabaseName = "db",
                ContainerName = "test"
            };

            var inventoryServiceOptions = new InventoryServiceOptions
            {
                BaseUrl = "https://test.example.jp",
                GetFunctionKey = "test-function-key-get",
                PostFunctionKey = "test-function-key-post"
            };

            return MicrosoftExtOptions.Options.Create(new AppOptions
            {
                InventoryServiceOptions = inventoryServiceOptions,
                CosmosDbOptions = cosmosOptions
            });
        }

        private Mock<IHttpClientFactory> CreatMockHttpClientFactory(HttpResponseMessage testResponseMessage)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(testResponseMessage);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(m => m.CreateClient(It.IsAny<string>())).Returns(httpClient);

            return mockHttpClientFactory;
        }

        private InventoryDeviceModel CreateDeviceResponseModel(string deviceId)
        {
            return new InventoryDeviceModel
            {
                DeviceId = deviceId,
                AssetId = Guid.NewGuid().ToString()
            };
        }

        private InventoryDeviceListModel CreateDeviceListResponseModel(string[] deviceIds)
        {
            var devices = new List<InventoryDeviceModel>();

            foreach (var deviceId in deviceIds)
            {
                devices.Add(new InventoryDeviceModel { DeviceId = deviceId, AssetId = Guid.NewGuid().ToString() });
            };

            return new InventoryDeviceListModel { Devices = devices };
        }

        private HttpResponseMessage CreateSuccessResponse<T>(T responseModel)
        {
            var content = JsonContent.Create(responseModel);

            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = content
            };
        }

        private HttpResponseMessage CreateErrorResponse()
        {
            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.NotFound,
            };
        }
    }
}