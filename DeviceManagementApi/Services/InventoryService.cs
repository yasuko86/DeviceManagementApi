using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManagementApi.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly HttpClient _client;

        public InventoryService(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
        }
    }
}
