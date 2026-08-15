using System;
using System.Collections.Generic;
using System.Text;

namespace Axpo.Infrastructure.Time
{
    public static class TimeZoneHelper
    {
        public static TimeZoneInfo GetLondonTimeZone()
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
