using Axso.Application.Configuration;
using Axso.Application.Services;
using Microsoft.Extensions.Options;

namespace Axso
{
    public class ReportWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ReportOptions _options;
        private readonly ILogger<ReportWorker> _logger;

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public ReportWorker(IServiceScopeFactory scopeFactory, IOptions<ReportOptions> options, ILogger<ReportWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started. Interval: {IntervalMinutes} minutes", _options.IntervalMinutes);

            await DoWork(stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.IntervalMinutes));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWork(stoppingToken);
            }
        }

        private async Task DoWork(CancellationToken ct)
        {
            if (!await _semaphore.WaitAsync(0, ct))
            {
                _logger.LogWarning("Previous report generation is still running. Skipping this execution.");

                return;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();

                var reportGenerator = scope.ServiceProvider.GetRequiredService<ReportGenerator>();

                await reportGenerator.GenerateAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Report generation failed. The next scheduled execution will retry.");
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
