using System.Text.Json.Serialization;

namespace PowerManagerConfig
{
    public sealed class MqttConfiguration
    {
        [JsonPropertyName("server_addr")]
        public string Server_Addr { get; set; } = string.Empty;

        [JsonPropertyName("server_port")]
        public string Server_Port { get; set; } = string.Empty;

        [JsonPropertyName("ssl_support")]
        public string Ssl_Support { get; set; } = "no";

        [JsonPropertyName("ssid")]
        public string Ssid { get; set; } = string.Empty;

        [JsonPropertyName("pass")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("mqtt_key")]
        public string Mqtt_Key { get; set; } = string.Empty;

        [JsonPropertyName("company")]
        public string Company { get; set; } = "DAWONDNS";

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("topic")]
        public string Topic { get; set; } = "dwd";
    }
}
