using CommandLine;
using System.Reflection;

namespace PowerManagerConfig
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Configuration>(args).WithParsed(config =>
            {
                PrintRevision(Console.Out);

                Console.Write("Mode V1 (B540 <= v1.01.26) or V2 (B540 == v1.01.28) or V3 (B540 == v1.01.30) or V4 (B550) or RECONFIG (default V1): ");
                string? mode = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(mode))
                    mode = "V1";

                IConfigrator configrator;
                IRestService restService;
                switch (mode)
                {
                    case "V1":
                        restService = new IRestService.RestService();
                        configrator = new IConfigrator.ConfigratorV1();
                        break;
                    default:
                        throw new Exception($"unknown mode '{mode}'");
                }
                configrator.Initialize(config, restService, Console.In, Console.Out);
                configrator.Configure();
                configrator.Dispose();
            })
            .WithNotParsed(errors =>
            {
                if (errors.IsVersion())
                    PrintRevision(errors.Output());
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
