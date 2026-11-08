using FitZone.Application.DTOs.PaginationDTOs;
using FitZone.Application.DTOs.UserDTOs;
using FitZone.Application.ServiceInterfaces;
using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FitZone.Application.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty) throw new EnterProperIdException();

            var user = await _userRepository.GetByIdAsync(id, ct);
            if (user is null || user.DeletedAt is not null)
                throw new UserNotFoundException();

            if (user.Role?.RoleName == "Admin")
                throw new ForbiddenException();

            if (await _userRepository.HasActiveFutureBookingsAsync(id, ct))
                throw new UserHasActiveBookingsException();

            user.MarkAsDeleted();
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("User {UserId} was deleted by an admin.", id);
        }

        public async Task<PagedResponseDTO<UserListItemDTO>> GetAllAsync(string? search, PaginationQueryDTO paging, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(paging);

            if (paging.Page < 1 || paging.PageSize < 1 || paging.PageSize > 50)
                throw new PageSizeException();

            var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

            var (users, total) = await _userRepository.GetPagedAsync(term, paging.Page, paging.PageSize, ct);

            return new PagedResponseDTO<UserListItemDTO>
            {
                TotalCount = total,
                Values = users.Select(u => new UserListItemDTO
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role?.RoleName ?? string.Empty,
                    IsActive = u.IsActive,
                    IsEmailVerified = u.IsEmailVerified,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt
                }).ToList()
            };
        }
    }
}
