using Axso.Domain.Models;

namespace Axso.Domain.Interfaces
{
    public interface ITradeRepository
    {
        Task<IReadOnlyList<Trade>> GetTradesAsync(DateTime date, CancellationToken ct = default);
    }
}
