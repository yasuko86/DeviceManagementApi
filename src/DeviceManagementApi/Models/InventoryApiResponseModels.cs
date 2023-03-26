using System.Collections.Generic;

namespace DeviceManagementApi.Models
{
    public class InventoryDeviceModel
    {
        public string DeviceId { get; set; }
        public string AssetId { get; set; }
    }

    public class InventoryDeviceListModel
    {
        public List<InventoryDeviceModel> Devices { get; set; }
    }
}
