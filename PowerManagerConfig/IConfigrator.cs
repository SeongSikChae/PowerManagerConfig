using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;
using System.Text.Json;

namespace PowerManagerConfig
{
    public sealed class NotInitialzedException : Exception
    {
    }

    public interface IConfigrator : IDisposable
    {
        void Initialize(Configuration config, IRestService restService, TextReader reader, TextWriter writer);

        void Configure();

        /// <summary>
        /// M130, B540(V1.0.26) 을 위한 Configrator
        /// </summary>
        public sealed class ConfigratorV1 : IConfigrator
        {
            public void Initialize(Configuration config, IRestService restService, TextReader reader, TextWriter writer)
            {
                this.config = config;
                this.restService = restService;
                this.reader = reader;
                this.writer = writer;
                socket.Connect(IPAddress.Parse(config.DeviceIP), config.DevicePort);
                writer.WriteLine($"{socket.RemoteEndPoint} Connected");
            }

            private Configuration? config;
            private IRestService? restService;
            private TextReader? reader;
            private TextWriter? writer;
            private readonly Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            public void Configure()
            {
                if (disposedValue)
                    throw new ObjectDisposedException(nameof(socket));
                if (config is null || restService is null)
                    throw new NotInitialzedException();

                byte[] buf = new byte[1500];
                string mac = string.Empty;
                {
                    byte[] startMessageBlock = Encoding.ASCII.GetBytes("[DUT<-PC] START\n");
                    socket.Send(startMessageBlock, 0, startMessageBlock.Length, SocketFlags.None);

                    byte[] helloMessageBlock = Encoding.ASCII.GetBytes("hello tcp SUCCESS_CONNECT\n");
                    socket.Send(helloMessageBlock, 0, helloMessageBlock.Length, SocketFlags.None);

                    int receiveLength = socket.Receive(buf, 0, buf.Length, SocketFlags.None);
                    string responseString = Encoding.UTF8.GetString(buf, 0, receiveLength);
                    Console.WriteLine(responseString);

                    Regex regex = new Regex(@"\[DUT->PC\] START_OK:(?<Mac>\S+)#");
                    Match match = regex.Match(responseString);
                    if (match.Success)
                        mac = match.Groups[1].Value;
                }

                MqttConfiguration mqttConfig = new MqttConfiguration();
                
                writer?.Write("Server : ");
                string? input = reader?.ReadLine();
                if (input is null)
                    throw new ArgumentNullException(nameof(mqttConfig.Server_Addr));
                mqttConfig.Server_Addr = input;

                writer?.Write("Port : ");
                input = reader?.ReadLine();
                if (input is null)
                    throw new ArgumentNullException(nameof(mqttConfig.Server_Port));
                mqttConfig.Server_Port = input;

                writer?.Write("SSL : no or yes (default: no)");
                input = reader?.ReadLine();
                mqttConfig.Ssl_Support = string.IsNullOrEmpty(input) ? "no" : input;

                writer?.Write("SSID : ");
                input = reader?.ReadLine();
                if (input is null)
                    throw new ArgumentNullException(nameof(mqttConfig.Ssid));
                mqttConfig.Ssid = input;

                writer?.Write("Wifi Password : ");
                input = reader?.ReadLine();
                if (input is null)
                    throw new ArgumentNullException(nameof(mqttConfig.Password));
                mqttConfig.Password = input;

                writer?.Write("Model : ");
                input = reader?.ReadLine();
                if (input is null)
                    throw new ArgumentNullException(nameof(mqttConfig.Model));
                mqttConfig.Model = input;

                writer?.Write("UserId : ");
                input = reader?.ReadLine();
                if (input is null)
                    throw new ArgumentNullException(nameof(input));
                string user_id = input;

                Task<MqttAuth?> mqttAuthTask = restService.GetMqttAuth(config, new MqttAuthRequest
                {
                    AccountInfo = new MqttAuthRequest.Account
                    {
                        UserId = user_id
                    },
                    DeviceInfo = new MqttAuthRequest.Device
                    {
                        Mac = mac,
                        ModelId = mqttConfig.Model,
                        Company = "DAWONDNS",
                        Latitude = "0",
                        Longitude = "0",
                        Verify = "false"
                    }
                });
                mqttAuthTask.Wait();
                MqttAuth? mqttAuth = mqttAuthTask.Result;
                if (mqttAuth is null)
                    throw new ArgumentNullException(nameof(MqttAuth));
                writer?.WriteLine($"mqtt_key: {mqttAuth.MqttKey}");

                writer?.Write($"Mqtt Server Password : ({mqttAuth.MqttKey})");
                input = reader?.ReadLine();
                string? mqtt_key = string.IsNullOrEmpty(input) ? mqttAuth.MqttKey : input;
                if (mqtt_key is null)
                    throw new ArgumentNullException(nameof(mqtt_key));
                mqttConfig.Mqtt_Key = mqtt_key;

                writer?.Write("Mqtt Topic : ");
                input = reader?.ReadLine();
                mqttConfig.Topic = string.IsNullOrEmpty(input) ? "dwd" : input;

                string json = JsonSerializer.Serialize(mqttConfig);
                writer?.WriteLine($"Push: {json}");
                int writeLength =socket.Send(Encoding.ASCII.GetBytes(json + "\n"));
                writer?.WriteLine($"Send: {writeLength} Byte.");

                try
                {
                    socket.Close();
                }
                finally
                {
                    socket.Dispose();
                }

                writer?.Write("네트워크를 정상화 시킨 후 Enter를 입력하세요.");
                reader?.ReadLine();

                Task<string> mqttAuthAddTask = restService.MqttAuthAdd(config, user_id, mac, string.IsNullOrEmpty(mqttAuth.Verify) ? string.Empty : mqttAuth.Verify, mqttConfig.Mqtt_Key);
                mqttAuthAddTask.Wait();
                writer?.WriteLine(mqttAuthAddTask.Result);

                Task<string> mqttKeyChangeTask = restService.MqttKeyChange(config, mac, mqttConfig.Mqtt_Key, config.ClientCertificate is null ? null : new FileInfo(config.ClientCertificate), config.ClientCertificatePassword);
                mqttKeyChangeTask.Wait();
                writer?.WriteLine(mqttKeyChangeTask.Result);
            }

            private bool disposedValue;

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                    }
                    socket.Dispose();

                    disposedValue = true;
                }
            }

            ~ConfigratorV1()
            {
                Dispose(disposing: false);
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        
    }
}
