using DeviceManagementApi.Models;
using DeviceManagementApi.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManagementApi.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly HttpClient _client;
        private readonly InventoryServiceOptions _serviceOptions;

        private const string getAssetId = "/assetId";

        private readonly string workingDirectory = Environment.CurrentDirectory;
        private readonly string dummyResponseTextFile = "DummyResponseContents.txt";

        public InventoryService(IHttpClientFactory httpClientFactory, IOptions<AppOptions> appOptions)
        {
            _client = httpClientFactory.CreateClient();
            _serviceOptions = appOptions?.Value.InventoryServiceOptions ?? throw new ArgumentNullException(nameof(appOptions));
        }

        public async Task<InventoryDeviceModel> GetAsync(string devideId)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{_serviceOptions.BaseUrl}{getAssetId}/{devideId}");
            httpRequest.Headers.Add("x-functions-key", _serviceOptions.GetFunctionKey);

            var response = await _client.SendAsync(httpRequest);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Failed in calling inventory endpoint with Status Code: [{response.StatusCode}]");
            }

            var jsonString = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<InventoryDeviceModel>(jsonString);
        }

        public async Task<InventoryDeviceListModel> PostAsync(string[] deviceIds)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_serviceOptions.BaseUrl}{getAssetId}");

            var property = new JProperty("deviceIds", JArray.FromObject(deviceIds));

            var requestJson = new JObject
            {
                property
            };

            var body = JsonConvert.SerializeObject(requestJson);
            
            httpRequest.Content = new StringContent(body, Encoding.UTF8, "application/json");
            httpRequest.Headers.Add("x-functions-key", _serviceOptions.PostFunctionKey);

            //var response = await _client.SendAsync(httpRequest);

            //if (response.StatusCode != System.Net.HttpStatusCode.OK)
            //{
            //    throw new Exception($"Failed in calling inventory endpoint with Status Code: [{response.StatusCode}]");
            //}

            //var jsonString = await response.Content.ReadAsStringAsync();

            var jsonString = await File.ReadAllTextAsync(Path.Combine(workingDirectory, dummyResponseTextFile));

            return JsonConvert.DeserializeObject<InventoryDeviceListModel>(jsonString);
        }
    }
}
