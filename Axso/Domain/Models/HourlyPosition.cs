using System;
using System.Collections.Generic;
using System.Text;

namespace Axso.Domain.Models
{
    public class HourlyPosition
    {
        public int Hour { get; }

        public double Volume { get; }

        public HourlyPosition(int hour, double volume)
        {
            Hour = hour;
            Volume = volume;
        }
    }
}
