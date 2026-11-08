using FitZone.Application.DTOs.PaginationDTOs;
using FitZone.Application.DTOs.UserDTOs;

namespace FitZone.Application.ServiceInterfaces
{
    public interface IUserService
    {
        Task<PagedResponseDTO<UserListItemDTO>> GetAllAsync(string? search, PaginationQueryDTO paging, CancellationToken ct = default);

        // ადმინი წაშლის მომხმარებელს (soft delete); Admin-ის წაშლა აკრძალულია, მომავალი ჯავშნის მქონეს ვერ წაშლის
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
