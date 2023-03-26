using DeviceManagementApi.Models;
using DeviceManagementApi.Options;
using DeviceManagementApi.Services;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net.Http.Json;

namespace DeviceManagementApi.Tests.Integration
{
    public static class MockServiceCreator
    {
        private static int inventoryServiceSlaSeconds = 30;

        public static InventoryService CreateMockInventoryService(IOptions<AppOptions> appOptions, string[] deviceIds)
        {
            var reponseContentModel = CreateDeviceListResponseModel(deviceIds);

            var mockResponse = CreatePostSuccessResponse(reponseContentModel);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback(() => Thread.Sleep(inventoryServiceSlaSeconds * 1000))
                .ReturnsAsync(mockResponse);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(m => m.CreateClient(It.IsAny<string>())).Returns(httpClient);

            return new InventoryService(mockHttpClientFactory.Object, appOptions);
        }

        private static InventoryDeviceListModel CreateDeviceListResponseModel(string[] deviceIds)
        {
            var devices = new List<InventoryDeviceModel>();

            foreach (var deviceId in deviceIds)
            {
                devices.Add(new InventoryDeviceModel { DeviceId = deviceId, AssetId = Guid.NewGuid().ToString() });
            };

            return new InventoryDeviceListModel { Devices = devices };
        }

        private static HttpResponseMessage CreatePostSuccessResponse(InventoryDeviceListModel responseModel)
        {
            var content = JsonContent.Create(responseModel);

            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = content
            };
        }
    }
}
