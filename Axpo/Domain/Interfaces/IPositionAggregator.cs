using Axpo.Domain.Models;

namespace Axpo.Domain.Interfaces
{
    public interface IPositionAggregator
    {
        IReadOnlyList<HourlyPosition> Aggregate(IReadOnlyList<Trade> trades, CancellationToken ct = default);
    }
}
