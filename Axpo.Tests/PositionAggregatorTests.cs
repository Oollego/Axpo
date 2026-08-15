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

        // Deterministic in-memory trade (no SDK): index 0 = period 1.
        private static Trade InMemoryTrade(DateTime date, params double[] volumesByPeriod)
        {
            var periods = volumesByPeriod
                .Select((volume, index) => new TradePeriod(index + 1, volume))
                .ToList();

            return new Trade(date, periods);
        }

        [Fact]
        public void Aggregate_MatchesChallengeExample()
        {
            var date = new DateTime(2015, 1, 4);

            var trade1 = InMemoryTrade(date, Enumerable.Repeat(100.0, 24).ToArray());

            // Trade 2 from the spec: periods 1-11 = 50, periods 12-24 = -20.
            var trade2 = InMemoryTrade(date, Enumerable.Range(1, 24)
                .Select(p => p <= 11 ? 50.0 : -20.0)
                .ToArray());

            var result = _aggregator.Aggregate(new[] { trade1, trade2 });

            var expectedHours = new[] { 23, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22 };
            var expectedVolumes = Enumerable.Range(1, 24).Select(p => p <= 11 ? 150.0 : 80.0).ToArray();

            Assert.Equal(24, result.Count);
            for (int i = 0; i < 24; i++)
            {
                Assert.Equal(expectedHours[i], result[i].Hour);
                Assert.Equal(expectedVolumes[i], result[i].Volume);
            }
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
        public void Aggregate_ShouldSumVolumesAcrossTrades_ForKnownInput()
        {
            // Expected sums are hardcoded, not re-derived with the same GroupBy().Sum()
            // the aggregator uses, so a wrong sum actually fails the test.
            var date = new DateTime(2026, 7, 15);

            var trades = new[]
            {
                InMemoryTrade(date, 10, 20, 30),
                InMemoryTrade(date, 1, 2, 3),
                InMemoryTrade(date, -5, 0, 100),
            };

            var result = _aggregator.Aggregate(trades);

            Assert.Equal(3, result.Count);
            Assert.Equal(6, result[0].Volume);    // 10 + 1 - 5
            Assert.Equal(22, result[1].Volume);   // 20 + 2 + 0
            Assert.Equal(133, result[2].Volume);  // 30 + 3 + 100
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