using Axso.Application.Configuration;
using Axso.Domain.Interfaces;
using Axso.Domain.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Axso.Infrastructure.Services
{
    public class CsvReportWriter : IReportWriter
    {
        private readonly ReportOptions _options;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<CsvReportWriter> _logger;

        public CsvReportWriter(IOptions<ReportOptions> options, IDateTimeProvider dateTimeProvider, ILogger<CsvReportWriter> logger)
        {
            _options = options.Value;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<string> WriteAsync(IReadOnlyList<HourlyPosition> positions, CancellationToken ct = default)
        {
            var now = _dateTimeProvider.Now;

            var fileName = $"PowerPosition_{now:yyyyMMdd}_{now:HHmm}.csv";

            var filePath = Path.Combine(_options.OutputFolder, fileName);

            try
            {
                Directory.CreateDirectory(_options.OutputFolder);

                using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

                await writer.WriteLineAsync("Local Time,Volume".AsMemory(), ct);

                foreach (var position in positions)
                {
                    ct.ThrowIfCancellationRequested();

                    var line = $"{position.Hour:D2}:00,{position.Volume.ToString(CultureInfo.InvariantCulture)}";

                    await writer.WriteLineAsync(line.AsMemory(), ct);
                }

                _logger.LogInformation("CSV report written to {FilePath}", filePath);

                return filePath;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogError(ex, "Output directory not found.");

                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "No permission to write report.");

                throw;
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error writing CSV report.");

                throw;
            }
        }
    }
}
