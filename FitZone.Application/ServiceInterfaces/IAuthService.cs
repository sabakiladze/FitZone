using FitZone.Application.DTOs.Auth;
using FitZone.Application.DTOs.AuthDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.ServiceInterfaces
{
    public interface IAuthService
    {
        Task<RegisterResponseDTO> RegisterAsync(RegisterRequestDTO request, CancellationToken ct = default);
        Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request, CancellationToken ct = default);
        Task DeleteUserAsync(Guid userId, DeleteAccountRequestDTO request, CancellationToken ct = default);
        Task<bool> VerifyEmailAsync(VerificationEmailRequestDTO request, CancellationToken ct = default);
        Task LogOutAsync(string token, CancellationToken ct = default);

        Task<AuthResponseDTO> RefreshTokenAsync(string hashToken, CancellationToken ct = default);

    }
}
