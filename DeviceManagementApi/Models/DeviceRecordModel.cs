using Newtonsoft.Json;

namespace DeviceManagementApi.Models
{
    public class DeviceRecordModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("assetId")]
        public string? AssetId { get; set; }
    }
}
