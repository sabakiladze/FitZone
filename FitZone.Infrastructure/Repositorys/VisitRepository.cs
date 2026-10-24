using FitZone.Domain.Entities.Models;
using FitZone.Domain.RepositoryInterfaces;
using FitZone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Infrastructure.Repositorys
{
    public class VisitRepository : IVisitRepository
    {
        private readonly FitZoneDbContext _fitZone;
        public VisitRepository(FitZoneDbContext fitZone)
        {
            _fitZone = fitZone;
        }
        public async Task AddAsync(Visit visit, CancellationToken ct = default)
        {
            await _fitZone.Visits.AddAsync(visit, ct);

        }

        public async Task<ICollection<Visit>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _fitZone.Visits
                .AsNoTracking()
                .Include(x=>x.UserMembershipPlan)
                .ThenInclude(x=>x.MembershipPlan)
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.CheckedInAt)
                .ToListAsync(ct);
        }
    }
}
