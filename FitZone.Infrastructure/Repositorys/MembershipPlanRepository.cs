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
    public class MembershipPlanRepository : IMembershipPlanRepository
    {
        private readonly FitZoneDbContext _fitZoneDbContext;
        public MembershipPlanRepository(FitZoneDbContext fitZoneDbContext)
        {
            _fitZoneDbContext = fitZoneDbContext;
        }
        public async Task<ICollection<MembershipPlan>> GetAllMembersipPlanAsync(CancellationToken ct = default)
        {
            return await  _fitZoneDbContext.MembershipPlans.AsNoTracking().ToListAsync(ct);
        }

        public async Task<MembershipPlan?> GetbyIdMembershipPlan(Guid id, CancellationToken ct = default)
        {
            return await _fitZoneDbContext.MembershipPlans.AsNoTracking().FirstOrDefaultAsync( x => x.Id == id, ct);
        }
        public async Task<MembershipPlan?> GetTrackedByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _fitZoneDbContext.MembershipPlans.FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken ct = default)
        {
            return await _fitZoneDbContext.MembershipPlans
                .AnyAsync(p => p.Name == name && (excludeId == null || p.Id != excludeId), ct);
        }

        public async Task<bool> HasUserMembershipsAsync(Guid planId, CancellationToken ct = default)
        {
            return await _fitZoneDbContext.MembershipPlans
                .AnyAsync(p => p.Id == planId && p.UserMembershipPlans.Any(), ct);
        }
        public async Task AddAsync(MembershipPlan plan, CancellationToken ct = default)
        { 
           await _fitZoneDbContext.MembershipPlans.AddAsync(plan, ct);
        }


        public void Remove(MembershipPlan plan)
        { _fitZoneDbContext.MembershipPlans.Remove(plan); }
    }
}
