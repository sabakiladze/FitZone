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
    public class UserMembershipPlanRepository : IUserMembershipRepository
    {
        private readonly FitZoneDbContext _fitZoneDbContext;
        public UserMembershipPlanRepository(FitZoneDbContext fitZoneDbContext)
        {
            _fitZoneDbContext = fitZoneDbContext;
        }
        public async Task AddAsync(UserMembershipPlan membership, CancellationToken ct = default)
        {
            await _fitZoneDbContext.UserMembershipPlans.AddAsync(membership, ct);
        }

        public async Task<UserMembershipPlan?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _fitZoneDbContext.UserMembershipPlans
                .AsNoTracking()
                .Include(x => x.MembershipPlan) // აქ ეს იმიტომ მჭირდება რომ მერე dto-ში პლან ის სახელი დავწერო
                .Where(x => x.UserId == userId && x.EndDate > DateTime.UtcNow)
                .OrderByDescending(m => m.EndDate)
                .FirstOrDefaultAsync(x => x.UserId == userId, ct);

            // ეს აბრუნებს ობიექტს რომლის ვადა და აიდი ემთხვევა, ანუ ვადა კიდევ აქვს მის გეგმას
        }
    }
}
