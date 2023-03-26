namespace DeviceManagementApi.Options
{
    public class AppOptions
    {
        public InventoryServiceOptions InventoryServiceOptions { get; set; }
        public CosmosDbOptions CosmosDbOptions { get; set; }
    }

    public class InventoryServiceOptions
    {
        public string BaseUrl { get; set; }
        public string GetFunctionKey { get; set; }
        public string PostFunctionKey { get; set; }
    }

    public class CosmosDbOptions
    {
        public string Uri { get; set; }
        public string Key { get; set; }
        public string DatabaseName { get; set; }
        public string ContainerName { get; set; }
    }
}
