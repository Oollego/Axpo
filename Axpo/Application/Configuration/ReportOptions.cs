using System.ComponentModel.DataAnnotations;

namespace Axpo.Application.Configuration
{
    public class ReportOptions
    {
        public string OutputFolder { get; set; } = "Reports";

        [Range(1, 1440)]
        public int IntervalMinutes { get; set; } = 30;
    }
}
