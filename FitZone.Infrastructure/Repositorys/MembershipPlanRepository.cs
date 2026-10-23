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
            return await  _fitZoneDbContext.MembershipPlans.ToListAsync(ct);
        }

        public async Task<MembershipPlan?> GetbyIdMembershipPlan(Guid id, CancellationToken ct = default)
        {
            return await _fitZoneDbContext.MembershipPlans.FirstOrDefaultAsync( x => x.Id == id, ct);
        }
    }
}
