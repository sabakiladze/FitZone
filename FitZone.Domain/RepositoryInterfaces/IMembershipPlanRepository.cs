using FitZone.Domain.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.RepositoryInterfaces
{
    public interface IMembershipPlanRepository
    {
        Task<ICollection<MembershipPlan>> GetAllMembersipPlanAsync( CancellationToken ct=default);
        Task<MembershipPlan?> GetbyIdMembershipPlan(Guid id, CancellationToken ct=default);

        Task<MembershipPlan?> GetTrackedByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken ct = default);
        Task<bool> HasUserMembershipsAsync(Guid planId, CancellationToken ct = default);
        Task AddAsync(MembershipPlan plan, CancellationToken ct = default);
        void Remove(MembershipPlan plan);

    }
}
