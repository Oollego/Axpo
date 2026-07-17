using System;
using System.Collections.Generic;
using System.Text;

namespace Axso.Domain.Models
{
    public class TradePeriod
    {
        public int Period { get; }

        public double Volume { get; }

        public TradePeriod(int period, double volume)
        {
            Period = period;
            Volume = volume;
        }
    }
}
