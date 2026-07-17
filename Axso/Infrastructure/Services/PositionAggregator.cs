using Axso.Domain.Interfaces;
using Axso.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axso.Infrastructure.Services
{
    public class PositionAggregator : IPositionAggregator
    {
        public IReadOnlyList<HourlyPosition> Aggregate(IReadOnlyList<Trade> trades, CancellationToken ct = default)
        {
            if (trades.Count == 0)
            {
                return Array.Empty<HourlyPosition>();
            }

            var date = trades[0].Date;

            var startLocalTime = new DateTime(date.Year, date.Month, date.Day, 23, 0, 0).AddDays(-1);

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

                    var localTime = startLocalTime.AddHours(kvp.Key - 1);

                    return new HourlyPosition(localTime.Hour, kvp.Value);
                })
                .ToList();
        }
    }
}
