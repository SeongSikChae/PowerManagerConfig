using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace PowerManagerConfig.Tests
{
    [TestClass]
    public class ReConfigratorTests
    {
        [TestMethod]
        public async Task ConfigureAsyncTest()
        {
            MockRestService restService = new MockRestService();
            TestMockDeviceCommunicator deviceCommunicator = new TestMockDeviceCommunicator();
            TestMockReader reader = new TestMockReader();
            await reader.InitializeAsync();
            IConfigrator.ReConfigrator configrator = new IConfigrator.ReConfigrator();
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
                queue.Enqueue("test@outlook.com/kakao");
                queue.Enqueue("000000");
                queue.Enqueue("B550_W");
                queue.Enqueue("true");
                queue.Enqueue("true");
                await Task.CompletedTask;
            }
        }

        private sealed class TestMockDeviceCommunicator : AbstractMockDeviceCommunicator
        {
            public override async Task InitializeAsync(Configuration config, TextWriter writer)
            {
                await Task.CompletedTask;
            }

            public override Task<string> ReceiveMessageAsync()
            {
                throw new NotImplementedException();
            }

            public override Task SendConnactApRequestAsync(ConnactApRequest connactApRequestMessage)
            {
                throw new NotImplementedException();
            }

            public override Task SendDelayMessageAsync(DelayMessage delayMessage)
            {
                throw new NotImplementedException();
            }
        }
    }
}