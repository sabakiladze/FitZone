using FitZone.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.AuthDTOs
{
    public class LoginResponseDTO
    {
        public AuthResponseDTO? AuthResponseDto { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
