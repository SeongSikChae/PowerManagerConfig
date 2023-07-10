namespace PowerManagerConfig.Tests
{
    internal sealed class MockRestService : IRestService
    {
        public async Task<MqttAuth?> GetMqttAuthAsync(Configuration config, MqttAuthRequest req)
        {
            return await Task.FromResult(new MqttAuth
            {
                MqttKey = "key",
                Verify = "verify"
            });
        }

        public async Task<string> MqttAuthAddAsync(Configuration config, string userId, string mac, string verify, string? mqttKey)
        {
            return await Task.FromResult("MqttAuthAdd");
        }

        public async Task<string> MqttKeyChangeAsync(Configuration config, string mac, string? mqttKey, FileInfo? clientCertificateFile = null, string? clientCertificatePassword = null)
        {
            return await Task.FromResult("MqttKeyChange");
        }
    }
}
