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

    }
}
