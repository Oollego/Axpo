using Axso.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axso.Domain.Interfaces
{
    public interface IReportWriter
    {
        Task<string> WriteAsync(IReadOnlyList<HourlyPosition> positions, CancellationToken ct = default);
    }
}
