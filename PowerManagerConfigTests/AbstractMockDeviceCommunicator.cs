namespace PowerManagerConfig.Tests
{
    internal abstract class AbstractMockDeviceCommunicator : IDeviceCommunicator
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
            return await Task.FromResult("000000");
        }

        public async Task<int> SendConfigrationAsync(string config)
        {
            return await Task.FromResult(0);
        }

        public abstract Task<string> ReceiveMessageAsync();

        public abstract Task SendDelayMessageAsync(string delayMessage);

        public async Task CloseAsync()
        {
            await Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}
