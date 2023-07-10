using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PowerManagerConfig.Tests
{
    [TestClass]
    public class ConfigratorV2Tests
    {
        [TestMethod]
        public async Task ConfigureTest()
        {
            MockRestService restService = new MockRestService();
            TestMockDeviceCommunicator deviceCommnicator = new TestMockDeviceCommunicator();
            TestMockReader reader = new TestMockReader();
            await reader.InitializeAsync();
            IConfigrator.ConfigratorV2 configrator = new IConfigrator.ConfigratorV2();
            await configrator.InitializeAsync(new Configuration
            {
                DeviceIP = "127.0.0.1",
                DevicePort = 55000,
            }, restService, deviceCommnicator, reader, new TraceWriter());
            await configrator.ConfigureAsync();
        }

        private sealed class TestMockReader : AbstractMockTextReader
        {
            public override async Task InitializeAsync()
            {
                queue.Enqueue(string.Empty);
                queue.Enqueue(string.Empty);
                queue.Enqueue(string.Empty);
                queue.Enqueue(string.Empty);
                queue.Enqueue(string.Empty);
                queue.Enqueue("test_wifi");
                queue.Enqueue("1234");
                queue.Enqueue("B540_W");
                queue.Enqueue("test@outlook.com/kakao");
                queue.Enqueue(string.Empty);
                queue.Enqueue(string.Empty);
                await Task.CompletedTask;
            }
        }

        private sealed class TestMockDeviceCommunicator : AbstractMockDeviceCommunicator
        {
            public override async Task<string> ReceiveMessageAsync()
            {
                return await Task.FromResult("ReceiveMessage");
            }

            public override async Task SendDelayMessageAsync(string delayMessage)
            {
                await Task.CompletedTask;
            }
        }
    }
}