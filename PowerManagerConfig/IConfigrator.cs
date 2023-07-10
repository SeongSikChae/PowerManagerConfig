using System.Text.Json;

namespace PowerManagerConfig
{
    public sealed class NotInitialzedException : Exception
    {
    }

    public interface IConfigrator : IDisposable
    {
        Task InitializeAsync(Configuration config, IRestService restService, IDeviceCommunicator deviceCommunicator, TextReader reader, TextWriter writer);

        Task ConfigureAsync();

        public static readonly IConfigrator Null = new NullConfigrator();

        public abstract class AbstractConfigrator : IConfigrator
        {
            public async Task InitializeAsync(Configuration config, IRestService restService, IDeviceCommunicator deviceCommunicator, TextReader reader, TextWriter writer)
            {
                this.config = config;
                this.restService = restService;
                this.reader = reader;
                this.writer = writer;
                this.deviceCommunicator = deviceCommunicator;
                await deviceCommunicator.InitializeAsync(config, writer);
            }

            protected Configuration? config;
            protected IRestService restService = IRestService.Null;
            protected TextReader reader = TextReader.Null;
            protected TextWriter writer = TextWriter.Null;
            protected IDeviceCommunicator deviceCommunicator = IDeviceCommunicator.Null;

            public abstract Task ConfigureAsync();

            protected bool disposedValue;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                    }
                    deviceCommunicator.Dispose();

                    disposedValue = true;
                }
            }

            ~AbstractConfigrator()
            {
                Dispose(disposing: false);
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        private sealed class NullConfigrator : AbstractConfigrator
        {
            public override async Task ConfigureAsync()
            {
                await Task.CompletedTask;
            }
        }

        /// <summary>
        /// M130, B540(V1.0.26) 을 위한 Configrator
        /// </summary>
        public sealed class ConfigratorV1 : AbstractConfigrator
        {
            public override async Task ConfigureAsync()
            {
                if (disposedValue)
                    throw new ObjectDisposedException(nameof(deviceCommunicator));
                if (config is null || restService is null || deviceCommunicator is null)
                    throw new NotInitialzedException();

                deviceCommunicator.SendStartMessageAsync().Wait();
                deviceCommunicator.SendHelloMessageAsync().Wait();

                MqttConfiguration mqttConfig = new MqttConfiguration();

                string mac = await deviceCommunicator.ReceiveDeviceMacAsync();

                await writer.WriteAsync("Mqtt Server_Addr: (default: dwmqtt.dawonai.com)");
                mqttConfig.Server_Addr = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.Server_Addr))
                    mqttConfig.Server_Addr = "dwmqtt.dawonai.com";

                await writer.WriteAsync("Mqtt Server_Port: (default: 8883)");
                mqttConfig.Server_Port = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.Server_Port))
                    mqttConfig.Server_Port = "8883";

                await writer.WriteAsync("SSL : no or yes (default: yes)");
                mqttConfig.Ssl_Support = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.Ssl_Support))
                    mqttConfig.Ssl_Support = "yes";

                await writer.WriteAsync("SSID : ");
                mqttConfig.Ssid = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.Ssid))
                    throw new ArgumentNullException(nameof(mqttConfig.Ssid));

                await writer.WriteAsync("Wifi Password : ");
                mqttConfig.Password = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.Password))
                    throw new ArgumentNullException(nameof(mqttConfig.Password));

                await writer.WriteAsync("Model : ");
                mqttConfig.Model = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.Model))
                    throw new ArgumentNullException(nameof(mqttConfig.Model));

                await writer.WriteAsync("UserId : ");
                string? user_id = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(user_id))
                    throw new ArgumentNullException(nameof(user_id));

                MqttAuth? mqttAuth = await restService.GetMqttAuthAsync(config, new MqttAuthRequest
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
                if (mqttAuth is null)
                    throw new ArgumentNullException(nameof(MqttAuth));
                await writer.WriteLineAsync($"mqtt_key: {mqttAuth.MqttKey}");

                await writer.WriteAsync($"Mqtt Server Password : ({mqttAuth.MqttKey})");
                mqttConfig.Mqtt_Key = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(mqttConfig.Mqtt_Key))
                    mqttConfig.Mqtt_Key = mqttAuth.MqttKey;

                await writer.WriteAsync("Mqtt Topic : (default: dwd)");
                mqttConfig.Topic = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(mqttConfig.Topic))
                    mqttConfig.Topic = "dwd";

                int sendBytes = await deviceCommunicator.SendConfigrationAsync(mqttConfig);
                await writer.WriteLineAsync($"Send: {sendBytes} Byte.");

                await deviceCommunicator.CloseAsync();

                await writer.WriteLineAsync("네트워크를 정상화 시킨 후 Enter를 입력하세요.");
                await reader.ReadLineAsync();

                string msg = await restService.MqttAuthAddAsync(config, user_id, mac, string.IsNullOrEmpty(mqttAuth.Verify) ? string.Empty : mqttAuth.Verify, mqttConfig.Mqtt_Key);
                await writer.WriteLineAsync(msg);

                msg = await restService.MqttKeyChangeAsync(config, mac, mqttConfig.Mqtt_Key, config.ClientCertificate is null ? null : new FileInfo(config.ClientCertificate), config.ClientCertificatePassword);
                await writer.WriteLineAsync(msg);
            }
        }

        /// <summary>
        /// B540 (v1.0.28) 을 위한 Configrator
        /// </summary>
        public sealed class ConfigratorV2 : AbstractConfigrator
        {
            public override async Task ConfigureAsync()
            {
                if (disposedValue)
                    throw new ObjectDisposedException(nameof(deviceCommunicator));
                if (config is null || restService is null || deviceCommunicator is null)
                    throw new NotInitialzedException();

                deviceCommunicator.SendStartMessageAsync().Wait();
                deviceCommunicator.SendHelloMessageAsync().Wait();

                MqttConfigurationV2 mqttConfig = new MqttConfigurationV2();

                string mac = await deviceCommunicator.ReceiveDeviceMacAsync();

                await writer.WriteAsync($"Device Mac: (default: {mac})");
                mqttConfig.Mac = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.Mac))
                    mqttConfig.Mac = mac;

                await writer.WriteAsync("ApiServerAddr: (default: dwapi.dawonai.com)");
                mqttConfig.ApiServerAddr = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.ApiServerAddr))
                    mqttConfig.ApiServerAddr = "dwapi.dawonai.com";

                await writer.WriteAsync("ApiServerPort: (default: 18443)");
                mqttConfig.ApiServerPort = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.ApiServerPort))
                    mqttConfig.ApiServerPort = "18443";

                await writer.WriteAsync("Mqtt Server_Addr: (default: dwmqtt.dawonai.com)");
                mqttConfig.Server_Addr = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.Server_Addr))
                    mqttConfig.Server_Addr = "dwmqtt.dawonai.com";

                await writer.WriteAsync("Mqtt Server_Port: (default: 8883)");
                mqttConfig.Server_Port = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.Server_Port))
                    mqttConfig.Server_Port = "8883";

                await writer.WriteAsync("SSID : ");
                mqttConfig.Ssid = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.Ssid))
                    throw new ArgumentNullException(nameof(mqttConfig.Ssid));

                await writer.WriteAsync("Wifi Password : ");
                mqttConfig.Password = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.Password))
                    throw new ArgumentNullException(nameof(mqttConfig.Password));

                await writer.WriteAsync("Model : ");
                mqttConfig.Model = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.Model))
                    throw new ArgumentNullException(nameof(mqttConfig.Model));

                await writer.WriteAsync("UserId : ");
                mqttConfig.UserId = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.UserId))
                    throw new ArgumentNullException(nameof(mqttConfig.UserId));

                await writer.WriteAsync("Mqtt Topic : (default: dwd)");
                mqttConfig.Topic = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(mqttConfig.Topic))
                    mqttConfig.Topic = "dwd";

                int sendBytes = await deviceCommunicator.SendConfigrationAsync(mqttConfig);
                await writer.WriteLineAsync($"Send: {sendBytes} Byte.");

                string? msg = await deviceCommunicator.ReceiveMessageAsync();
                await writer.WriteLineAsync(msg);
                await deviceCommunicator.SendDelayMessageAsync(new DelayMessage());

                await deviceCommunicator.CloseAsync();

                await writer.WriteLineAsync("네트워크를 정상화 시킨 후 Enter를 입력하세요.");
                await reader.ReadLineAsync();

                MqttAuth? mqttAuth = await restService.GetMqttAuthAsync(config, new MqttAuthRequest
                {
                    AccountInfo = new MqttAuthRequest.Account
                    {
                        UserId = mqttConfig.UserId
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
                if (mqttAuth is null)
                    throw new ArgumentNullException(nameof(MqttAuth));
                await writer.WriteLineAsync($"mqtt_key: {mqttAuth.MqttKey}");
                

                msg = await restService.MqttAuthAddAsync(config, mqttConfig.UserId, mac, string.IsNullOrEmpty(mqttAuth.Verify) ? string.Empty : mqttAuth.Verify, mqttAuth.MqttKey);
                await writer.WriteLineAsync(msg);

                msg = await restService.MqttKeyChangeAsync(config, mac, mqttAuth.MqttKey, config.ClientCertificate is null ? null : new FileInfo(config.ClientCertificate), config.ClientCertificatePassword);
                await writer.WriteLineAsync(msg);
            }
        }

        /// <summary>
        /// B540 (v1.0.30, v1.0.32) 을 위한 Configrator
        /// </summary>
        public sealed class ConfigratorV3 : AbstractConfigrator
        {
            public override async Task ConfigureAsync()
            {
                if (disposedValue)
                    throw new ObjectDisposedException(nameof(deviceCommunicator));
                if (config is null || restService is null || deviceCommunicator is null)
                    throw new NotInitialzedException();

                deviceCommunicator.SendStartMessageAsync().Wait();
                deviceCommunicator.SendHelloMessageAsync().Wait();

                MqttConfigurationV2 mqttConfig = new MqttConfigurationV2();


                string mac = await deviceCommunicator.ReceiveDeviceMacAsync();

                await writer.WriteAsync($"Device Mac: (default: {mac})");
                mqttConfig.Mac = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.Mac))
                    mqttConfig.Mac = mac;

                await writer.WriteAsync("SSID : ");
                mqttConfig.Ssid = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.Ssid))
                    throw new ArgumentNullException(nameof(mqttConfig.Ssid));

                await writer.WriteAsync("Wifi Password : ");
                mqttConfig.Password = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.Password))
                    throw new ArgumentNullException(nameof(mqttConfig.Password));

                await writer.WriteAsync("Model : ");
                mqttConfig.Model = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.Model))
                    throw new ArgumentNullException(nameof(mqttConfig.Model));

                await writer.WriteAsync("UserId : ");
                mqttConfig.UserId = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(mqttConfig.UserId))
                    throw new ArgumentNullException(nameof(mqttConfig.UserId));

                await writer.WriteAsync("Mqtt Topic : (default: dwd)");
                mqttConfig.Topic = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(mqttConfig.Topic))
                    mqttConfig.Topic = "dwd";

                int sendBytes = await deviceCommunicator.SendConfigrationAsync(mqttConfig);
                await writer.WriteLineAsync($"Send: {sendBytes} Byte.");

                string? msg = await deviceCommunicator.ReceiveMessageAsync();
                await writer.WriteLineAsync(msg);

                await deviceCommunicator.SendConnactApRequestAsync(new ConnactApRequest
                {
                    Mac = mqttConfig.Mac
                });

                msg = await deviceCommunicator.ReceiveMessageAsync();
                await writer.WriteLineAsync(msg);

                await deviceCommunicator.CloseAsync();

                await writer.WriteLineAsync("네트워크를 정상화 시킨 후 Enter를 입력하세요.");
                await reader.ReadLineAsync();

                MqttAuth? mqttAuth = await restService.GetMqttAuthAsync(config, new MqttAuthRequest
                {
                    AccountInfo = new MqttAuthRequest.Account
                    {
                        UserId = mqttConfig.UserId
                    },
                    DeviceInfo = new MqttAuthRequest.Device
                    {
                        Mac = mac,
                        ModelId = mqttConfig.Model,
                        Company = "DAWONDNS",
                        Latitude = "0",
                        Longitude = "0",
                        Verify = "true"
                    }
                });
                if (mqttAuth is null)
                    throw new ArgumentNullException(nameof(MqttAuth));
                await writer.WriteLineAsync($"mqtt_key: {mqttAuth.MqttKey}");


                msg = await restService.MqttAuthAddAsync(config, mqttConfig.UserId, mac, string.IsNullOrEmpty(mqttAuth.Verify) ? string.Empty : mqttAuth.Verify, mqttAuth.MqttKey);
                await writer.WriteLineAsync(msg);

                msg = await restService.MqttKeyChangeAsync(config, mac, mqttAuth.MqttKey, config.ClientCertificate is null ? null : new FileInfo(config.ClientCertificate), config.ClientCertificatePassword);
                await writer.WriteLineAsync(msg);
            }
        }
    }
}
