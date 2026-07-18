using Axpo;
using Axpo.Application.Configuration;
using Axpo.Application.Services;
using Axpo.Domain.Interfaces;
using Axpo.Infrastructure.Data;
using Axpo.Infrastructure.Services;
using Axpo.Infrastructure.Time;

namespace Axpo
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApp(this IServiceCollection services)
        {
            services.AddOptions<ReportOptions>()
                    .BindConfiguration("ReportOptions")
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

            services.AddSingleton<IDateTimeProvider, LondonDateTimeProvider>();
            services.AddScoped<IPositionAggregator, PositionAggregator>();
            services.AddScoped<IReportWriter, CsvReportWriter>();
            services.AddSingleton<IPowerService, PowerService>();
            services.AddScoped<ITradeRepository, PowerTradeRepository>();
            services.AddScoped<ReportGenerator>();

            return services;
        }
    }
}
