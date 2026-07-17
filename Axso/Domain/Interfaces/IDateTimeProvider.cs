using System;
using System.Collections.Generic;
using System.Text;

namespace Axso.Domain.Interfaces
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
}
