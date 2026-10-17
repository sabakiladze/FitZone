using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.Auth
{
    public class AuthResponse
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public DateTime AccessTokenExpiresAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
