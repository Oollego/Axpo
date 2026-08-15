using System.Globalization;
using Axpo.Application.Configuration;
using Axpo.Domain.Interfaces;
using Axpo.Domain.Models;
using Axpo.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Axpo.Tests
{
    // Covers requirements 3 and 4 (CSV format and file name) — the literal deliverable.
    // Writes to a throwaway temp folder and reads it back.
    public sealed class CsvReportWriterTests : IDisposable
    {
        private sealed class FixedClock : IDateTimeProvider
        {
            public FixedClock(DateTime now) => Now = now;
            public DateTime Now { get; }
        }

        private readonly string _dir =
            Path.Combine(Path.GetTempPath(), "AxpoCsvTests", Guid.NewGuid().ToString("N"));

        private CsvReportWriter CreateWriter(DateTime now)
        {
            var options = Options.Create(new ReportOptions { OutputFolder = _dir, IntervalMinutes = 30 });
            return new CsvReportWriter(options, new FixedClock(now), NullLogger<CsvReportWriter>.Instance);
        }

        [Fact]
        public async Task Writes_Header_HhMmFormat_And_LeadingZeroHour()
        {
            var writer = CreateWriter(new DateTime(2015, 1, 4, 18, 37, 0));

            var path = await writer.WriteAsync(new[]
            {
                new HourlyPosition(23, 150),
                new HourlyPosition(1, 80),   // must be zero-padded to 01:00
            });

            var lines = await File.ReadAllLinesAsync(path);

            Assert.Equal("Local Time,Volume", lines[0]);
            Assert.Equal("23:00,150", lines[1]);
            Assert.Equal("01:00,80", lines[2]);
        }

        [Fact]
        public async Task FileName_UsesLocalExtractTime_InRequiredPattern()
        {
            var writer = CreateWriter(new DateTime(2015, 1, 4, 18, 37, 0));

            var path = await writer.WriteAsync(Array.Empty<HourlyPosition>());

            Assert.Equal("PowerPosition_20150104_1837.csv", Path.GetFileName(path));
        }

        [Fact]
        public async Task Volume_UsesDotDecimal_EvenUnderCommaCulture()
        {
            var original = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo("uk-UA"); // decimal separator = ','
            try
            {
                var writer = CreateWriter(new DateTime(2015, 1, 4, 18, 37, 0));

                var path = await writer.WriteAsync(new[] { new HourlyPosition(23, 1234.5) });

                var line = (await File.ReadAllLinesAsync(path))[1];

                Assert.Equal("23:00,1234.5", line); // dot, not comma — CSV stays parseable
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        public void Dispose()
        {
            if (Directory.Exists(_dir))
            {
                Directory.Delete(_dir, recursive: true);
            }
        }
    }
}
