using FitZone.Application.DTOs.MembershipDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.ServiceInterfaces
{
    public interface IMembershipService
    {
        Task<ICollection<MembershipPlanDTO>> GetPlansAsync(CancellationToken ct = default);

        Task<UserMembershipDTO> ActivateAsync(Guid userId, ActivateMembershipRequestDTO request, CancellationToken ct = default);

        Task<UserMembershipDTO?> GetMyActiveAsync(Guid userId, CancellationToken ct = default);

        Task<MembershipPlanDTO> CreatePlanAsync(CreateMembershipPlanRequestDTO request, CancellationToken ct = default);
        Task UpdatePlanAsync(Guid id, UpdateMembershipPlanRequestDTO request, CancellationToken ct = default);
        Task DeletePlanAsync(Guid id, CancellationToken ct = default);

    }
}
