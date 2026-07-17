using Axso.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axso.Domain.Interfaces
{
    public interface IPositionAggregator
    {
        IReadOnlyList<HourlyPosition> Aggregate(IReadOnlyList<Trade> trades, CancellationToken ct = default);
    }
}
