using CommandLine;

namespace PowerManagerConfig
{
    public sealed class Configuration
    {
        [Option("host", Required = true, HelpText = "device ip")]
        public string DeviceIP { get; set; } = string.Empty;

        [Option("port", Required = true, HelpText = "device port")]
        public ushort DevicePort { get; set; }

        [Option("web_server_addr", Required = true, HelpText = "Web Server IP Or Domain ex) https://{address}:{port}")]
        public string WebServerAddr { get; set; } = string.Empty;

        [Option("clientCertificate", Required = false, HelpText = "client certificate file path")]
        public string? ClientCertificate { get; set; }


        [Option("clientCertificatePassword", Required = false, HelpText = "client certificate password")]
        public string? ClientCertificatePassword { get; set; }
    }
}
