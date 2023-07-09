using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PowerManagerConfig.Tests
{
    [TestClass]
    public class ConfigratorV1Tests
    {
        [TestMethod]
        public async Task ConfigureTest()
        {
            DummyRestService restService = new DummyRestService();
            DummyDeviceCommnicator deviceCommnicator = new DummyDeviceCommnicator();
            FakeTextReader reader = new FakeTextReader();
            await reader.InitializeAsync();
            IConfigrator.ConfigratorV1 configrator = new IConfigrator.ConfigratorV1();
            await configrator.InitializeAsync(new Configuration
            {
                DeviceIP = "127.0.0.1",
                DevicePort = 55000,
            }, restService, deviceCommnicator, reader, new TraceWriter());
            await configrator.ConfigureAsync();
        }


        private sealed class TraceWriter : TextWriter
        {
            public override Encoding Encoding => Encoding.UTF8;

            public override void Write(string? value)
            {
                Trace.WriteLine(value);
            }

            public override void WriteLine(string? value)
            {
                Trace.WriteLine(value);
            }
        }

        private sealed class FakeTextReader : TextReader
        {
            public async Task InitializeAsync()
            {
                queue.Enqueue("dwmqtt.dawonai.com");
                queue.Enqueue("8883");
                queue.Enqueue("yes");
                queue.Enqueue("test_wifi");
                queue.Enqueue("1234");
                queue.Enqueue("B540_W");
                queue.Enqueue("test@outlook.com/kakao");
                queue.Enqueue("");
                queue.Enqueue("");
                queue.Enqueue("");
                await Task.CompletedTask;
            }

            private readonly Queue<string> queue = new Queue<string>();

            public override async Task<string?> ReadLineAsync()
            {
                return await Task.FromResult(queue.Count > 0 ? queue.Dequeue() : null);
            }
        }

        private sealed class DummyRestService : IRestService
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

        private sealed class DummyDeviceCommnicator : IDeviceCommunicator
        {
            public async Task InitializeAsync(Configuration config, TextWriter writer)
            {
                await writer.WriteLineAsync($"{config.DeviceIP}:{config.DevicePort} Connected");
                await Task.CompletedTask;
            }

            public async Task SendStartMessageAsync()
            {
                await Task.CompletedTask;
            }

            public async Task SendHelloMessageAsync()
            {
                await Task.CompletedTask;
            }

            public async Task<string> ReceiveDeviceMacAsync()
            {
                return await Task.FromResult("[DUT->PC] START_OK:000000#");
            }

            public async Task<int> SendConfigrationAsync(string config)
            {
                return await Task.FromResult(0);
            }

            public void Dispose()
            {
            }
        }
    }
}