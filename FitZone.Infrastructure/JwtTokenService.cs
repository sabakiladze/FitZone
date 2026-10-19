using FitZone.Application.ServiceInterfaces;
using FitZone.Domain.Entities.Models;
using FitZone.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Infrastructure
{
    public class JwtTokenService : IJwtTokenGenerator
    {
        private readonly JwtSettings _jwtSettings;
        public JwtTokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }
        public string GenerateAccessToken(User User)
        {
            var claims=new List<Claim>
            {
                new Jw
            }
        }

        public string GenerateRefreshToken()
        {
            throw new NotImplementedException();
        }
    }
}
