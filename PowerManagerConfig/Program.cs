using CommandLine;
using System.Reflection;

namespace PowerManagerConfig
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ParserResult<Configuration> result = await Parser.Default.ParseArguments<Configuration>(args).WithParsedAsync<Configuration>(async config =>
            {
                PrintRevision(Console.Out);

                Console.Write("Mode V1 (B540 <= v1.01.26) or V2 (B540 == v1.01.28) or V3 (B540 == v1.01.30) or V4 (B550) or RECONFIG (default V1): ");
                string? mode = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(mode))
                    mode = "V1";

                IConfigrator configrator = IConfigrator.Null;
                IRestService restService = IRestService.Null;
                IDeviceCommunicator deviceCommunicator = IDeviceCommunicator.Null;
                switch (mode)
                {
                    case "V1":
                        restService = new IRestService.RestService();
                        configrator = new IConfigrator.ConfigratorV1();
                        deviceCommunicator = new IDeviceCommunicator.DeviceCommunicator();
                        break;
                    default:
                        throw new Exception($"unknown mode '{mode}'");
                }
                await configrator.InitializeAsync(config, restService, deviceCommunicator, Console.In, Console.Out);
                await configrator.ConfigureAsync();
                configrator.Dispose();
            });

            await result.WithNotParsedAsync(async errors =>
            {
                if (errors.IsVersion())
                    PrintRevision(errors.Output());
                await Task.CompletedTask;
            });
        }

        private static void PrintRevision(TextWriter writer) 
        {
            RevisionAttribute? attr = typeof(RevisionAttribute).Assembly.GetCustomAttribute<RevisionAttribute>();
            if(attr is not null)
            {
                writer.WriteLine($"Revision: {attr.Revision}");
            }
        }
    }
}
