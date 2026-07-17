using System;
using System.Collections.Generic;
using System.Text;

namespace Axso.Domain.Models
{
    public class Trade
    {
        public DateTime Date { get; }

        public IReadOnlyList<TradePeriod> Periods { get; }

        public Trade(DateTime date, IReadOnlyList<TradePeriod> periods)
        {
            Date = date;
            Periods = periods;
        }
    }
}
