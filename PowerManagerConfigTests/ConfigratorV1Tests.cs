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
        public void ConfigureTest()
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 55000));
            socket.Listen();

            socket.BeginAccept(Accept, socket);

            try
            {
                DummyRestService restService = new DummyRestService();
                FakeTextReader reader = new FakeTextReader();
                reader.Initialize();
                IConfigrator.ConfigratorV1 configrator = new IConfigrator.ConfigratorV1();
                configrator.Initialize(new Configuration
                {
                    DeviceIP = "127.0.0.1",
                    DevicePort = 55000,
                }, restService, reader, new TraceWriter());
                configrator.Configure();
            }
            finally
            {
                socket.Dispose();
            }
        }

        private void Accept(IAsyncResult ar)
        {
            Socket? socket = ar.AsyncState as Socket;
            if (socket is null)
                Assert.Fail();
            Socket clientSocket = socket.EndAccept(ar);

            byte[] buffer = new byte[1500];
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Receive, new Tuple<Socket, byte[]>(clientSocket, buffer));
        }

        private void Receive(IAsyncResult ar)
        {
            Tuple<Socket, byte[]>? tuple = ar.AsyncState as Tuple<Socket, byte[]>;
            if (tuple is null)
                Assert.Fail();
            Socket socket = tuple.Item1;
            byte[] buffer = tuple.Item2;

            int receiveBytes = socket.EndReceive(ar);
            string message = Encoding.ASCII.GetString(buffer, 0, receiveBytes);
            Trace.WriteLine(message);

            if (message.StartsWith("hello tcp SUCCESS_CONNECT"))
            {
                string sendMessage = "[DUT->PC] START_OK:000000#";
                socket.Send(Encoding.ASCII.GetBytes(sendMessage));
            } 
            else
            {
                byte[] buffer2 = new byte[1500];
                socket.BeginReceive(buffer2, 0, buffer2.Length, SocketFlags.None, Receive, new Tuple<Socket, byte[]>(socket, buffer2));
            }
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
            public void Initialize()
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
            }

            private readonly Queue<string> queue = new Queue<string>();

            public override string? ReadLine()
            {
                return queue.Count > 0 ? queue.Dequeue() : null;
            }
        }

        private sealed class DummyRestService : IRestService
        {
            public async Task<MqttAuth?> GetMqttAuth(Configuration config, MqttAuthRequest req)
            {
                return await Task.FromResult(new MqttAuth
                {
                    MqttKey = "key",
                    Verify = "verify"
                });
            }

            public async Task<string> MqttAuthAdd(Configuration config, string userId, string mac, string verify, string mqttKey)
            {
                return await Task.FromResult("MqttAuthAdd");
            }

            public async Task<string> MqttKeyChange(Configuration config, string mac, string mqttKey, FileInfo? clientCertificateFile = null, string? clientCertificatePassword = null)
            {
                return await Task.FromResult("MqttKeyChange");
            }
        }
    }
}