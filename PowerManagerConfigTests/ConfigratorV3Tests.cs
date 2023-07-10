using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace PowerManagerConfig.Tests
{
    [TestClass]
    public class ConfigratorV3Tests
    {
        [TestMethod]
        public async Task ConfigureAsyncTest()
        {
            MockRestService restService = new MockRestService();
            TestMockDeviceCommunicator deviceCommunicator = new TestMockDeviceCommunicator();
            TestMockReader reader = new TestMockReader();
            await reader.InitializeAsync();
            IConfigrator.ConfigratorV3 configrator = new IConfigrator.ConfigratorV3();
            await configrator.InitializeAsync(new Configuration
            {
                DeviceIP = "127.0.0.1",
                DevicePort = 55000,
            }, restService, deviceCommunicator, reader, new TraceWriter());
            await configrator.ConfigureAsync();
        }

        private sealed class TestMockReader : AbstractMockTextReader
        {
            public override async Task InitializeAsync()
            {
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

            public override async Task SendConnactApRequestAsync(ConnactApRequest connactApRequestMessage)
            {
                Trace.WriteLine("SendConnactApRequest");
                await Task.CompletedTask;
            }

            public override Task SendDelayMessageAsync(DelayMessage delayMessage)
            {
                throw new NotImplementedException();
            }
        }
    }
}