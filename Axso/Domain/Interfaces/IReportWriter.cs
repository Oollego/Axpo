using Axso.Domain.Models;

namespace Axso.Domain.Interfaces
{
    public interface IReportWriter
    {
        Task<string> WriteAsync(IReadOnlyList<HourlyPosition> positions, CancellationToken ct = default);
    }
}
