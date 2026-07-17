using Axpo;
using Axso.Domain.Interfaces;
using Axso.Domain.Models;

namespace Axso.Infrastructure.Data
{
    public class PowerTradeRepository : ITradeRepository
    {
        private readonly IPowerService _powerService;
        private readonly ILogger<PowerTradeRepository> _logger;

        public PowerTradeRepository(
            IPowerService powerService,
            ILogger<PowerTradeRepository> logger)
        {
            _powerService = powerService;
            _logger = logger;
        }

        public async Task<IReadOnlyList<Trade>> GetTradesAsync(DateTime date, CancellationToken ct = default)
        {
            const int maxAttempts = 3;

            Exception? lastException = null;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    _logger.LogInformation("Retrieving trades. Attempt {Attempt}/{MaxAttempts}", attempt, maxAttempts);

                    var powerTrades = await _powerService.GetTradesAsync(date);

                    var trades = powerTrades.Select(MapToDomain).ToList();

                    _logger.LogInformation("Retrieved {Count} trades", trades.Count);

                    return trades;
                }
                catch (PowerServiceException ex)
                {
                    lastException = ex;

                    _logger.LogWarning(ex, "PowerService failed on attempt {Attempt}/{MaxAttempts}", attempt, maxAttempts);

                    if (attempt == maxAttempts)
                    {
                        break;
                    }
     
                    await Task.Delay(TimeSpan.FromSeconds(2), ct);
                }
            }

            throw lastException!;
        }

        private static Trade MapToDomain(PowerTrade powerTrade)
        {
            var periods = powerTrade.Periods
                .Select(p => new TradePeriod(p.Period, p.Volume))
                .ToList();

            return new Trade(powerTrade.Date, periods);
        }
    }
}
