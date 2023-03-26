namespace DeviceManagementApi.Tests.Integration
{
    internal class TestConstants
    {
        internal const string DummyResponseTextFile = "Sample_Request.json";
        internal const string TestSettingsFile = "test.settings.json";
    }

    internal class TestSettings
    {
        public bool IsEncrypted { get; set; }
        public Dictionary<string, string> Values { get; set; }
    }
}
