using FitZone.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FitZoneDbContext _fitZoneDbContext;
        public UnitOfWork(FitZoneDbContext fitZoneDbContext)
        {
            _fitZoneDbContext = fitZoneDbContext;
        }
        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _fitZoneDbContext.SaveChangesAsync(ct);
        }
    }
}
