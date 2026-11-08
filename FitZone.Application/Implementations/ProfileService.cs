using FitZone.Application.DTOs.ProfileDTOs;
using FitZone.Application.ServiceInterfaces;
using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Interfaces;
using FitZone.Domain.RepositoryInterfaces;

namespace FitZone.Application.Implementations
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepository;

        public ProfileService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ProfileDTO> GetAsync(Guid userId, CancellationToken ct = default)
        {
            if (userId == Guid.Empty) throw new EnterProperIdException();

            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user is null || user.DeletedAt is not null)
                throw new UserNotFoundException();

            return new ProfileDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role?.RoleName ?? string.Empty,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
