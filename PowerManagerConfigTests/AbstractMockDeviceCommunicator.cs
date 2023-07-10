namespace PowerManagerConfig.Tests
{
    internal abstract class AbstractMockDeviceCommunicator : IDeviceCommunicator
    {
        public virtual async Task InitializeAsync(Configuration config, TextWriter writer)
        {
            this.writer = writer;
            await writer.WriteLineAsync($"{config.DeviceIP}:{config.DevicePort} Connected");
            await Task.CompletedTask;
        }

        private TextWriter writer = TextWriter.Null;

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
            return await Task.FromResult("000000");
        }

        public async Task<int> SendConfigrationAsync<T>(T config) where T : IMqttConfiguration
        {
            await writer.WriteLineAsync("Push: ");
            return await Task.FromResult(0);
        }

        public abstract Task<string> ReceiveMessageAsync();

        public abstract Task SendDelayMessageAsync(DelayMessage delayMessage);

        public abstract Task SendConnactApRequestAsync(ConnactApRequest connactApRequestMessage);

        public async Task CloseAsync()
        {
            await Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}
