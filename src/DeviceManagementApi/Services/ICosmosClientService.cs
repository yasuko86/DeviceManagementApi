using DeviceManagementApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceManagementApi.Services
{
    public interface ICosmosClientService
    {
        Task<List<string>> StoreDevices(List<DeviceRecordModel> devices);
    }
}
