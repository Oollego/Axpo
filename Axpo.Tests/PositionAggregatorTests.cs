using Axpo.Domain.Models;
using Axpo.Infrastructure.Data;
using Axpo.Infrastructure.Services;
using Microsoft.Extensions.Logging;



namespace Axpo.Tests
{
    public class PositionAggregatorTests
    {
        private readonly PositionAggregator _aggregator = new();

        public PositionAggregatorTests()
        {
            Environment.SetEnvironmentVariable("SERVICE_MODE", "Test");
        }

        private async Task<IReadOnlyList<Trade>> GetTradesAsync(DateTime date)
        {
            using var loggerFactory = LoggerFactory.Create(builder => { });

            var repository = new PowerTradeRepository(
                new PowerService(),
                loggerFactory.CreateLogger<PowerTradeRepository>());

            return await repository.GetTradesAsync(date);
        }

        [Fact]
        public async Task Aggregate_ShouldReturn23Rows_ForSpringDst()
        {
            var trades = await GetTradesAsync(new DateTime(2026, 3, 29));
            var result = _aggregator.Aggregate(trades);
            Assert.Equal(23, result.Count);
        }

        [Fact]
        public async Task Aggregate_ShouldReturn24Rows_ForNormalDay()
        {
            var trades = await GetTradesAsync(new DateTime(2026, 7, 15));
            var result = _aggregator.Aggregate(trades);
            Assert.Equal(24, result.Count);
        }

        [Fact]
        public async Task Aggregate_ShouldReturn25Rows_ForAutumnDst()
        {
            var trades = await GetTradesAsync(new DateTime(2026, 10, 25));
            var result = _aggregator.Aggregate(trades);
            Assert.Equal(25, result.Count);
        }

        [Theory]
        [InlineData(2026, 3, 29)]
        [InlineData(2026, 7, 15)]
        [InlineData(2026, 10, 25)]
        public async Task Aggregate_ShouldReturnSameNumberOfRows_AsPowerServicePeriods(
            int year,
            int month,
            int day)
        {
            var trades = await GetTradesAsync(new DateTime(year, month, day));
            var result = _aggregator.Aggregate(trades);
            Assert.Equal(trades.First().Periods.Count, result.Count);
        }

        [Fact]
        public async Task Aggregate_ShouldAggregateVolumes_ForEachPeriod()
        {
            var trades = await GetTradesAsync(new DateTime(2026, 7, 15));

            var expectedVolumes = trades
                .SelectMany(t => t.Periods)
                .GroupBy(p => p.Period)
                .OrderBy(g => g.Key)
                .Select(g => g.Sum(x => x.Volume))
                .ToArray();

            var result = _aggregator.Aggregate(trades);

            var actualVolumes = result
                .Select(x => x.Volume)
                .ToArray();

            Assert.Equal(expectedVolumes.Length, actualVolumes.Length);

            for (int i = 0; i < expectedVolumes.Length; i++)
            {
                Assert.Equal(expectedVolumes[i], actualVolumes[i], 10);
            }
        }

        [Fact]
        public async Task Aggregate_ShouldSkipHour1_ForSpringDst()
        {
            var trades = await GetTradesAsync(new DateTime(2026, 3, 29));
            var result = _aggregator.Aggregate(trades);
            Assert.DoesNotContain(result, x => x.Hour == 1);
        }

        [Fact]
        public async Task Aggregate_ShouldContainTwoHour1_ForAutumnDst()
        {
            var trades = await GetTradesAsync(new DateTime(2026, 10, 25));
            var result = _aggregator.Aggregate(trades);
            Assert.Equal(2, result.Count(x => x.Hour == 1));
        }

        [Fact]
        public async Task Aggregate_ShouldStartAt23_ForNormalDay()
        {
            var trades = await GetTradesAsync(new DateTime(2026, 7, 15));
            var result = _aggregator.Aggregate(trades);
            Assert.Equal(23, result.First().Hour);
            Assert.Equal(22, result.Last().Hour);
        }

        [Fact]
        public void Aggregate_ShouldReturnEmpty_WhenTradesAreEmpty()
        {
            var result = _aggregator.Aggregate(Array.Empty<Trade>());
            Assert.Empty(result);
        }
    }
}