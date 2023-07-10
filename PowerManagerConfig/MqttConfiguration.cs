using System.Text.Json.Serialization;

namespace PowerManagerConfig
{
    public interface IMqttConfiguration { }

    public sealed class MqttConfiguration : IMqttConfiguration
    {
        [JsonPropertyName("server_addr")]
        public string? Server_Addr { get; set; } = "dwmqtt.dawonai.com";

        [JsonPropertyName("server_port")]
        public string? Server_Port { get; set; } = "8883";

        [JsonPropertyName("ssl_support")]
        public string? Ssl_Support { get; set; } = "yes";

        [JsonPropertyName("ssid")]
        public string? Ssid { get; set; } = string.Empty;

        [JsonPropertyName("pass")]
        public string? Password { get; set; } = string.Empty;

        [JsonPropertyName("mqtt_key")]
        public string? Mqtt_Key { get; set; } = string.Empty;

        [JsonPropertyName("company")]
        public string Company { get; set; } = "DAWONDNS";

        [JsonPropertyName("model")]
        public string? Model { get; set; } = string.Empty;

        [JsonPropertyName("topic")]
        public string? Topic { get; set; } = "dwd";
    }

    public sealed class MqttConfigurationV2 : IMqttConfiguration
    {
        [JsonPropertyName("mac")]
        public string? Mac { get; set; }

        [JsonPropertyName("api_server_addr")]
        public string? ApiServerAddr { get; set; } = "dwapi.dawonai.com";

        [JsonPropertyName("api_server_port")]
        public string? ApiServerPort { get; set; } = "18443";

        [JsonPropertyName("server_addr")]
        public string? Server_Addr { get; set; } = "dwmqtt.dawonai.com";

        [JsonPropertyName("server_port")]
        public string? Server_Port { get; set; } = "8883";

        [JsonPropertyName("ssl_support")]
        public string Ssl_Support { get; set; } = "yes";

        [JsonPropertyName("ssid")]
        public string? Ssid { get; set; } = string.Empty;

        [JsonPropertyName("pass")]
        public string? Password { get; set; } = string.Empty;

        [JsonPropertyName("user_id")]
        public string? UserId { get; set; } = string.Empty;

        [JsonPropertyName("company")]
        public string Company { get; set; } = "DAWONDNS";

        [JsonPropertyName("model")]
        public string? Model { get; set; } = string.Empty;

        [JsonPropertyName("lati")]
        public string Latitude { get; set; } = "37.6523018";

        [JsonPropertyName("long")]
        public string Longitude { get; set; } = "127.0622559";

        [JsonPropertyName("topic")]
        public string? Topic { get; set; } = "dwd";
    }

    public sealed class DelayMessage
    {
        [JsonPropertyName("delay")]
        public string Delay { get; set; } = "0";
    }

    public sealed class ConnactApRequest
    {
        [JsonPropertyName("mac")]
        public string Mac { get; set; } = string.Empty;

        [JsonPropertyName("command")]
        public string Command { get; set; } = "connectap";
    }
}
