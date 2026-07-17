using Axso.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axso.Infrastructure.Time
{
    public class LondonDateTimeProvider : IDateTimeProvider
    {
        private readonly TimeZoneInfo _londonZone;

        public LondonDateTimeProvider()
        {
            _londonZone = GetLondonTimeZone();
        }

        public DateTime Now =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _londonZone);

        private static TimeZoneInfo GetLondonTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
            }
        }
    }
}
