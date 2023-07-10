using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PowerManagerConfig
{
    public interface IDeviceCommunicator : IDisposable 
    {
        Task InitializeAsync(Configuration config, TextWriter writer);

        Task SendStartMessageAsync();

        Task SendHelloMessageAsync();

        Task<string> ReceiveDeviceMacAsync();

        Task<int> SendConfigrationAsync<T>(T config) where T : IMqttConfiguration;

        Task<string> ReceiveMessageAsync();

        Task SendDelayMessageAsync(DelayMessage delayMessage);

        Task SendConnactApRequestAsync(ConnactApRequest connactApRequestMessage);

        Task CloseAsync();

        public static readonly IDeviceCommunicator Null = new NullDeviceCommunicator();

        private sealed class NullDeviceCommunicator : IDeviceCommunicator
        {
            public async Task InitializeAsync(Configuration conifg, TextWriter writer)
            {
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
                return await Task.FromResult(string.Empty);
            }

            public async Task<int> SendConfigrationAsync<T>(T config) where T : IMqttConfiguration
            {
                return await Task.FromResult(0);
            }

            public async Task<string> ReceiveMessageAsync()
            {
                return await Task.FromResult(string.Empty);
            }

            public async Task SendDelayMessageAsync(DelayMessage delayMessage)
            {
                await Task.CompletedTask;
            }

            public async Task SendConnactApRequestAsync(ConnactApRequest connactApRequestMessage)
            {
                await Task.CompletedTask;
            }

            public async Task CloseAsync()
            {
                await Task.CompletedTask;
            }

            private bool disposedValue;

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                    }

                    disposedValue = true;
                }
            }

            // ~NullDeviceCommunicator()
            // {
            //     Dispose(disposing: false);
            // }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        public sealed class DeviceCommunicator : IDeviceCommunicator
        {
            public async Task InitializeAsync(Configuration config, TextWriter writer)
            {
                this.writer = writer;
                await socket.ConnectAsync(IPAddress.Parse(config.DeviceIP), config.DevicePort);
                await writer.WriteLineAsync($"{socket.RemoteEndPoint} Connected");
            }

            private readonly Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            private TextWriter writer = TextWriter.Null;

            public async Task SendStartMessageAsync()
            {
                byte[] startMessageBlock = Encoding.ASCII.GetBytes("[DUT<-PC] START\n");
                await socket.SendAsync(startMessageBlock, SocketFlags.None);
            }

            public async Task SendHelloMessageAsync()
            {
                byte[] helloMessageBlock = Encoding.ASCII.GetBytes("hello tcp SUCCESS_CONNECT\n");
                await socket.SendAsync(helloMessageBlock, SocketFlags.None);
            }

            public async Task<string> ReceiveDeviceMacAsync()
            {
                byte[] buf = new byte[1500];
                int receiveLength = await socket.ReceiveAsync(buf, SocketFlags.None);
                string responseString = Encoding.ASCII.GetString(buf, 0, receiveLength);
                Regex regex = new Regex(@"\[DUT->PC\] START_OK:(?<Mac>\S+)#");
                Match match = regex.Match(responseString);
                if (match.Success)
                    return match.Groups[1].Value;
                return string.Empty;
            }

            public async Task<int> SendConfigrationAsync<T>(T config) where T : IMqttConfiguration
            {
                string json = JsonSerializer.Serialize(config);
                await writer.WriteLineAsync($"Push: {json}");
                return await socket.SendAsync(Encoding.ASCII.GetBytes(json + "\n"), SocketFlags.None);
            }

            public async Task<string> ReceiveMessageAsync()
            {
                byte[] buf = new byte[1500];
                int receiveBytes = await socket.ReceiveAsync(buf, SocketFlags.None);
                return Encoding.ASCII.GetString(buf, 0, receiveBytes);
            }

            public async Task SendDelayMessageAsync(DelayMessage delayMessage)
            {
                string json = JsonSerializer.Serialize(delayMessage);
                byte[] buf = Encoding.ASCII.GetBytes(json + "\n");
                await socket.SendAsync(buf, SocketFlags.None);
            }

            public async Task SendConnactApRequestAsync(ConnactApRequest connactApRequestMessage)
            {
                string json = JsonSerializer.Serialize(connactApRequestMessage);
                byte[] buf = Encoding.ASCII.GetBytes(json + "\n");
                await socket.SendAsync(buf, SocketFlags.None);
            }

            public async Task CloseAsync()
            {
                await socket.DisconnectAsync(false);
                Dispose(true);
            }

            private bool disposedValue;

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                    }

                    if (socket.Connected)
                        socket.Close();
                    socket.Dispose();
                    disposedValue = true;
                }
            }

            ~DeviceCommunicator()
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
