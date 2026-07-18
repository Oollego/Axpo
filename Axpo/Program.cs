namespace Axpo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            var switchMappings = new Dictionary<string, string>
            {
                ["--interval"] = "ReportOptions:IntervalMinutes",
                ["--output"] = "ReportOptions:OutputFolder"
            };

            builder.Configuration.AddCommandLine(args, switchMappings);

            builder.Services
                .AddApp()
                .AddHostedService<ReportWorker>();

            var host = builder.Build();

            await host.RunAsync();
        }
    }
}
