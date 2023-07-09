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

                string json = JsonSerializer.Serialize(mqttConfig);
                await writer.WriteLineAsync($"Push: {json}");

                int sendBytes = await deviceCommunicator.SendConfigrationAsync(json);
                await writer.WriteLineAsync($"Send: {sendBytes} Byte.");

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
        //public sealed class ConfigratorV2 : AbstractConfigrator
        //{
        //    public override void Configure()
        //    {
        //        if (disposedValue)
        //            throw new ObjectDisposedException(nameof(socket));
        //        if (config is null || restService is null)
        //            throw new NotInitialzedException();

        //        byte[] buf = new byte[1500];
        //        string mac = string.Empty;
        //        {
        //            byte[] startMessageBlock = Encoding.ASCII.GetBytes("[DUT<-PC] START\n");
        //            socket.Send(startMessageBlock, 0, startMessageBlock.Length, SocketFlags.None);

        //            byte[] helloMessageBlock = Encoding.ASCII.GetBytes("hello tcp SUCCESS_CONNECT\n");
        //            socket.Send(helloMessageBlock, 0, helloMessageBlock.Length, SocketFlags.None);

        //            int receiveLength = socket.Receive(buf, 0, buf.Length, SocketFlags.None);
        //            string responseString = Encoding.UTF8.GetString(buf, 0, receiveLength);
        //            Console.WriteLine(responseString);

        //            Regex regex = new Regex(@"\[DUT->PC\] START_OK:(?<Mac>\S+)#");
        //            Match match = regex.Match(responseString);
        //            if (match.Success)
        //                mac = match.Groups[1].Value;
        //        }

        //        MqttConfigurationV2 mqttConfig = new MqttConfigurationV2();

        //        if (string.IsNullOrWhiteSpace(mac))
        //        {
        //            writer?.Write("Mac: ");
        //            mqttConfig.Mac = reader?.ReadLine();
        //        }
        //        else
        //            mqttConfig.Mac = mac;

        //        writer?.Write("ApiServerAddr: (default: dwapi.dawonai.com)");
        //        mqttConfig.ApiServerAddr = reader?.ReadLine();
        //        if (string.IsNullOrWhiteSpace(mqttConfig.ApiServerAddr))
        //            mqttConfig.ApiServerAddr = "dwapi.dawonai.com";

        //        writer?.Write("ApiServerPort: (default: 18443)");
        //        mqttConfig.ApiServerPort = reader?.ReadLine();
        //        if (string.IsNullOrWhiteSpace(mqttConfig.ApiServerPort))
        //            mqttConfig.ApiServerPort = "18443";

        //        writer?.Write("Mqtt Server_Addr: (default: dwmqtt.dawonai.com)");
        //        mqttConfig.Server_Addr = reader?.ReadLine();
        //        if (string.IsNullOrWhiteSpace(mqttConfig.Server_Addr))
        //            mqttConfig.Server_Addr = "dwmqtt.dawonai.com";

        //        writer?.Write("Mqtt Server_Port: (default: 8883)");
        //        mqttConfig.Server_Port = reader?.ReadLine();
        //        if (string.IsNullOrWhiteSpace(mqttConfig.Server_Port))
        //            mqttConfig.Server_Port = "8883";

        //        writer?.Write("SSID : ");
        //        mqttConfig.Ssid = reader?.ReadLine();
        //        if (string.IsNullOrWhiteSpace(mqttConfig.Ssid))
        //            throw new ArgumentNullException(nameof(mqttConfig.Ssid));

        //        writer?.Write("Wifi Password : ");
        //        mqttConfig.Password = reader?.ReadLine();
        //        if (string.IsNullOrWhiteSpace(mqttConfig.Password))
        //            throw new ArgumentNullException(nameof(mqttConfig.Password));

        //        writer?.Write("UserId : ");
        //        mqttConfig.UserId = reader?.ReadLine();
        //        if (string.IsNullOrWhiteSpace(mqttConfig.UserId))
        //            throw new ArgumentNullException(nameof(mqttConfig.UserId));

        //        writer?.Write("Model : ");
        //        mqttConfig.Model = reader?.ReadLine();
        //        if (string.IsNullOrWhiteSpace(mqttConfig.Model))
        //            throw new ArgumentNullException(nameof(mqttConfig.Model));

        //        writer?.Write("Mqtt Topic : (default: dwd)");
        //        mqttConfig.Topic = reader?.ReadLine();
        //        if (string.IsNullOrEmpty(mqttConfig.Topic))
        //            mqttConfig.Topic = "dwd";

        //        string json = JsonSerializer.Serialize(mqttConfig);
        //        writer?.WriteLine($"Push: {json}");
        //        int writeLength = socket.Send(Encoding.ASCII.GetBytes(json + "\n"));
        //        writer?.WriteLine($"Send: {writeLength} Byte.");

        //        {
        //            Array.Clear(buf, 0, buf.Length);
        //            int len = socket.Receive(buf);
        //            writer?.WriteLine(Encoding.UTF8.GetString(buf, 0, len));
        //        }

        //        {
        //            DelayMessage delayMessage = new DelayMessage();
        //            string requestJson = JsonSerializer.Serialize(delayMessage);
        //            byte[] connectApBlock = Encoding.ASCII.GetBytes(requestJson + "\n");
        //            socket.Send(connectApBlock, 0, connectApBlock.Length, SocketFlags.None);
        //        }

        //        try
        //        {
        //            socket.Close();
        //        }
        //        finally
        //        {
        //            socket.Dispose();
        //        }

        //        writer?.Write("네트워크를 정상화 시킨 후 Enter를 입력하세요.");
        //        reader?.ReadLine();

        //        Task<MqttAuth?> mqttAuthTask = restService.GetMqttAuth(config, new MqttAuthRequest
        //        {
        //            AccountInfo = new MqttAuthRequest.Account
        //            {
        //                UserId = mqttConfig.UserId
        //            },
        //            DeviceInfo = new MqttAuthRequest.Device
        //            {
        //                Mac = mac,
        //                ModelId = mqttConfig.Model,
        //                Company = "DAWONDNS",
        //                Latitude = "0",
        //                Longitude = "0",
        //                Verify = "false"
        //            }
        //        });
        //        mqttAuthTask.Wait();
        //        MqttAuth? mqttAuth = mqttAuthTask.Result;
        //        if (mqttAuth is null)
        //            throw new ArgumentNullException(nameof(MqttAuth));
        //        writer?.WriteLine($"mqtt_key: {mqttAuth.MqttKey}");

        //        Task<string> mqttAuthAddTask = restService.MqttAuthAdd(config, mqttConfig.UserId, mac, string.IsNullOrEmpty(mqttAuth.Verify) ? string.Empty : mqttAuth.Verify, mqttAuth.MqttKey);
        //        mqttAuthAddTask.Wait();
        //        writer?.WriteLine(mqttAuthAddTask.Result);

        //        Task<string> mqttKeyChangeTask = restService.MqttKeyChange(config, mac, mqttAuth.MqttKey, config.ClientCertificate is null ? null : new FileInfo(config.ClientCertificate), config.ClientCertificatePassword);
        //        mqttKeyChangeTask.Wait();
        //        writer?.WriteLine(mqttKeyChangeTask.Result);
        //    }
        //}
    }
}
