﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PowerManagerConfig.Tests
{
    [TestClass]
    public class ConfigratorV1Tests
    {
        [TestMethod]
        public async Task ConfigureTest()
        {
            MockRestService restService = new MockRestService();
            TestMockDeviceCommunicator deviceCommnicator = new TestMockDeviceCommunicator();
            TestMockReader reader = new TestMockReader();
            await reader.InitializeAsync();
            IConfigrator.ConfigratorV1 configrator = new IConfigrator.ConfigratorV1();
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
                queue.Enqueue("test_wifi");
                queue.Enqueue("1234");
                queue.Enqueue("B540_W");
                queue.Enqueue("test@outlook.com/kakao");
                queue.Enqueue(string.Empty);
                queue.Enqueue(string.Empty);
                queue.Enqueue(string.Empty);
                await Task.CompletedTask;
            }
        }

        private sealed class TestMockDeviceCommunicator : AbstractMockDeviceCommunicator
        {
            public override Task<string> ReceiveMessageAsync()
            {
                throw new NotImplementedException();
            }

            public override Task SendDelayMessageAsync(DelayMessage delayMessage)
            {
                throw new NotImplementedException();
            }

            public override Task SendConnactApRequestAsync(ConnactApRequest connactApRequestMessage)
            {
                throw new NotImplementedException();
            }
        }
    }
}