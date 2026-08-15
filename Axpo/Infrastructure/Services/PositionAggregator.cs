using Axpo.Domain.Interfaces;
using Axpo.Domain.Models;
using Axpo.Infrastructure.Time;

namespace Axpo.Infrastructure.Services
{
    public class PositionAggregator : IPositionAggregator
    {
        private static readonly TimeZoneInfo LondonZone = TimeZoneHelper.GetLondonTimeZone();

        public IReadOnlyList<HourlyPosition> Aggregate(IReadOnlyList<Trade> trades, CancellationToken ct = default)
        {
            if (trades.Count == 0)
            {
                return Array.Empty<HourlyPosition>();
            }

            var date = trades[0].Date;

            var startLocal = new DateTime(date.Year, date.Month, date.Day, 23, 0, 0, DateTimeKind.Unspecified).AddDays(-1);
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, LondonZone);

            var volumesByPeriod = new SortedDictionary<int, double>();

            foreach (var trade in trades)
            {
                ct.ThrowIfCancellationRequested();

                foreach (var period in trade.Periods)
                {
                    if (volumesByPeriod.ContainsKey(period.Period))
                    {
                        volumesByPeriod[period.Period] += period.Volume;
                    }
                    else
                    {
                        volumesByPeriod[period.Period] = period.Volume;
                    }
                }
            }

            return volumesByPeriod
                .Select(kvp =>
                {
                    ct.ThrowIfCancellationRequested();

                    var periodUtc = startUtc.AddHours(kvp.Key - 1);
                    var localTime = TimeZoneInfo.ConvertTimeFromUtc(periodUtc, LondonZone);

                    return new HourlyPosition(localTime.Hour, kvp.Value);
                })
                .ToList();
        }
    }
}
