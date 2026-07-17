namespace Axso
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services
                .AddApp()
                .AddHostedService<ReportWorker>();

            var host = builder.Build();

            await host.RunAsync();
        }
    }
}
