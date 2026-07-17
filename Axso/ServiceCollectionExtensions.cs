using Axpo;
using Axso.Application.Configuration;
using Axso.Application.Services;
using Axso.Domain.Interfaces;
using Axso.Infrastructure.Data;
using Axso.Infrastructure.Services;
using Axso.Infrastructure.Time;

namespace Axso
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
