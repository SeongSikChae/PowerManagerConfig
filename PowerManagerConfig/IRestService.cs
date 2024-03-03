using System.Net;
using System.Net.Http.Json;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;

namespace PowerManagerConfig
{
    public sealed class MqttAuthRequest
    {
        [JsonPropertyName("account")]
        public Account? AccountInfo { get; set; }

        [JsonPropertyName("device")]
        public Device? DeviceInfo { get; set; }

        public sealed class Account
        {
            [JsonPropertyName("user_id")]
            public string? UserId { get; set; }
        }

        public sealed class Device
        {
            [JsonPropertyName("mac")]
            public string? Mac { get; set; }

            [JsonPropertyName("model_id")]
            public string? ModelId { get; set; }

            [JsonPropertyName("company")]
            public string? Company { get; set; }

            [JsonPropertyName("lati")]
            public string? Latitude { get; set; }

            [JsonPropertyName("long")]
            public string? Longitude { get; set; }

            [JsonPropertyName("verify")]
            public string? Verify { get; set; }
        }
    }

    public sealed class MqttAuth
    {
        [JsonPropertyName("verify")]
        public string? Verify { get; set; }

        [JsonPropertyName("mqtt_key")]
        public string? MqttKey { get; set; }
    }

    public sealed class AddAuthRequest
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("device_id")]
        public string DeviceId { get; set; } = string.Empty;

        [JsonPropertyName("verify")]
        public string Verify { get; set; } = string.Empty;

        [JsonPropertyName("mqtt_key")]
        public string? MqttKey { get; set; } = string.Empty;
    }

    public sealed class DeviceMqttKeyUpdateRequest
    {
        [JsonPropertyName("device_id")]
        public string DeviceId { get; set; } = string.Empty;

        [JsonPropertyName("mqtt_key")]
        public string? MqttKey { get; set; } = string.Empty;
    }

    public interface IRestService
    {
        Task<MqttAuth?> GetMqttAuthAsync(Configuration config, MqttAuthRequest req);

        Task<string> MqttAuthAddAsync(Configuration config, string userId, string mac, string verify, string? mqttKey);

        Task<string> MqttKeyChangeAsync(Configuration config, string mac, string? mqttKey, FileInfo? clientCertificateFile = null, string? clientCertificatePassword = null);

        public static readonly IRestService Null = new NullRestService();

        private sealed class NullRestService : IRestService
        {
            public async Task<MqttAuth?> GetMqttAuthAsync(Configuration config, MqttAuthRequest req)
            {
                return await Task.FromResult<MqttAuth?>(null);
            }

            public async Task<string> MqttAuthAddAsync(Configuration config, string userId, string mac, string verify, string? mqttKey)
            {
                return await Task.FromResult(string.Empty);
            }

            public async Task<string> MqttKeyChangeAsync(Configuration config, string mac, string? mqttKey, FileInfo? clientCertificateFile = null, string? clientCertificatePassword = null)
            {
                return await Task.FromResult(string.Empty);
            }
        }

        public sealed class RestService : IRestService
        {
            public async Task<MqttAuth?> GetMqttAuthAsync(Configuration config, MqttAuthRequest req)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using HttpClient client = new HttpClient();

                Uri uri = new Uri("https://dwapi.dawonai.com:18443/api/v1/devices/register/create");
                using HttpResponseMessage responseMessage = await client.PostAsJsonAsync(uri, req);
                return await responseMessage.Content.ReadFromJsonAsync<MqttAuth>();
            }

            public async Task<string> MqttAuthAddAsync(Configuration config, string userId, string mac, string verify, string? mqttKey) 
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using HttpClient client = new HttpClient();

                AddAuthRequest req = new AddAuthRequest
                {
                    UserId = userId,
                    DeviceId = mac,
                    Verify = verify,
                    MqttKey = mqttKey
                };

                Uri uri = new Uri($"{config.WebServerAddr}/api/auth/add");
                using HttpResponseMessage responseMessage = await client.PostAsJsonAsync(uri, req);
                return await responseMessage.Content.ReadAsStringAsync();
            }

            public async Task<string> MqttKeyChangeAsync(Configuration config, string mac, string? mqttKey, FileInfo? clientCertificateFile = null, string? clientCertificatePassword = null)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                using HttpClientHandler httpClientHandler = new HttpClientHandler();
                if (clientCertificateFile is not null && clientCertificatePassword is not null)
                {
                    httpClientHandler.ClientCertificates.Add(new X509Certificate2(clientCertificateFile.FullName, clientCertificatePassword));
                    httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                }
                else
                    httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;
                httpClientHandler.SslProtocols = SslProtocols.Tls12;

                using HttpClient client = new HttpClient(httpClientHandler);
                DeviceMqttKeyUpdateRequest request = new DeviceMqttKeyUpdateRequest
                {
                    DeviceId = mac,
                    MqttKey = mqttKey
                };
                Uri uri = new Uri($"{config.WebServerAddr}/rest/Auth/update_mqttKey");
                using HttpResponseMessage responseMessage = await client.PostAsJsonAsync(uri,request);
                return await responseMessage.Content.ReadAsStringAsync();
            }
        }
    }
}
