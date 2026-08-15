using Axpo.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axpo.Infrastructure.Time
{
    public class LondonDateTimeProvider : IDateTimeProvider
    {
        private readonly TimeZoneInfo _londonZone;

        public LondonDateTimeProvider()
        {
            _londonZone = TimeZoneHelper.GetLondonTimeZone();
        }

        public DateTime Now =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _londonZone);

    }
}
