using Axso.Domain.Models;

namespace Axso.Domain.Interfaces
{
    public interface IPositionAggregator
    {
        IReadOnlyList<HourlyPosition> Aggregate(IReadOnlyList<Trade> trades, CancellationToken ct = default);
    }
}
