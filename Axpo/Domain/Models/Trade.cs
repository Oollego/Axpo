
namespace Axpo.Domain.Models
{
    public class Trade
    {
        public DateTime Date { get; }

        public IReadOnlyList<TradePeriod> Periods { get; }

        public Trade(DateTime date, IReadOnlyList<TradePeriod> periods)
        {
            Date = date;
            Periods = periods;
        }
    }
}
