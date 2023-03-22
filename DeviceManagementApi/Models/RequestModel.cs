
using System.Collections.Generic;

namespace DeviceManagementApi.Models
{
    public class RequestModel
    {
        public string CorrelationId { get; set; }
        public List<DeviceModel> Devices { get; set; }
    }
}
