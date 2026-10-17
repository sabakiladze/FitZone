using FitZone.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.ServiceInterfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request, CancellationToken ct = default);
        Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request, CancellationToken ct = default);
        Task<bool> DeleteUserAsync(Guid userId, CancellationToken ct = default);
        Task<bool> VerifyEmailAsync(string email,string code, CancellationToken ct = default);
        Task LogOutAsync(string token, CancellationToken ct = default);

        Task<AuthResponseDTO> RefreshTokenAsync(string token, CancellationToken ct = default);

    }
}
