using DeviceManagementApi.Models;
using System.Threading.Tasks;

namespace DeviceManagementApi.Services
{
    public interface IInventoryService
    {
        Task<InventoryDeviceModel> GetAsync(string devideId);
        Task<InventoryDeviceListModel> PostAsync(string[] deviceIds);
    }
}
