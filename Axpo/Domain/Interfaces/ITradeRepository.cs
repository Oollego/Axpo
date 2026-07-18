using Axpo.Domain.Models;

namespace Axpo.Domain.Interfaces
{
    public interface ITradeRepository
    {
        Task<IReadOnlyList<Trade>> GetTradesAsync(DateTime date, CancellationToken ct = default);
    }
}
