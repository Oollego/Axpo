using Axpo.Domain.Models;

namespace Axpo.Domain.Interfaces
{
    public interface IReportWriter
    {
        Task<string> WriteAsync(IReadOnlyList<HourlyPosition> positions, CancellationToken ct = default);
    }
}
