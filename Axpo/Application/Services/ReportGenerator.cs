using Axpo.Domain.Interfaces;

namespace Axpo.Application.Services
{
    public class ReportGenerator
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly IPositionAggregator _aggregator;
        private readonly IReportWriter _writer;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<ReportGenerator> _logger;

        public ReportGenerator(
            ITradeRepository tradeRepository,
            IPositionAggregator aggregator,
            IReportWriter writer,
            IDateTimeProvider dateTimeProvider,
            ILogger<ReportGenerator> logger)
        {
            _tradeRepository = tradeRepository;
            _aggregator = aggregator;
            _writer = writer;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task GenerateAsync(CancellationToken ct = default)
        {
            var now = _dateTimeProvider.Now;
            var tradingDay = now.Hour >= 23 ? now.Date.AddDays(1) : now.Date;

            _logger.LogInformation("Generating report for {Date}", tradingDay);

            var trades = await _tradeRepository.GetTradesAsync(tradingDay, ct);

            if (trades.Count == 0)
            {
                _logger.LogWarning("No trades found for {Date}", tradingDay);
            }
            else
            {
                _logger.LogInformation(
                    "Retrieved {Trades} trades. Periods per trade: {Periods}",
                    trades.Count,
                    trades[0].Periods.Count);
            }

            var positions = _aggregator.Aggregate(trades, ct);

            _logger.LogInformation("Generated {Rows} report rows", positions.Count);

            await _writer.WriteAsync(positions, ct);

            _logger.LogInformation("Report generated");
        }
    }
}
