
using System.Collections.Generic;

namespace DeviceManagementApi.Models
{
    public class RequestModel
    {
        public string CorrelationId { get; set; }
        public List<DeviceModel> Devices { get; set; }
    }

    public class DeviceModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }

        public string Type { get; set; }
    }
}
