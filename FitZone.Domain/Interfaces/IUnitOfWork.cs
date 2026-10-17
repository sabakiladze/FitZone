using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.Interfaces
{
    public  interface IUnitOfWork
    {
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
