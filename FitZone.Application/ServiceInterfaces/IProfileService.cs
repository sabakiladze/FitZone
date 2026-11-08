using FitZone.Application.DTOs.ProfileDTOs;

namespace FitZone.Application.ServiceInterfaces
{
    public interface IProfileService
    {
        Task<ProfileDTO> GetAsync(Guid userId, CancellationToken ct = default);
    }
}
