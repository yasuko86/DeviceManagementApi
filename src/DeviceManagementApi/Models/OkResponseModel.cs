using System.Collections.Generic;

namespace DeviceManagementApi.Models
{
    public class OkResponseModel
    {
        public int TotalCount { get; set; }
        public int RegisterSuccessCount { get; set; }        
        public int RegisterFailureCount { get; set; }
        public List<DeviceModel>? FailedDevices { get; set; }
    }
}
