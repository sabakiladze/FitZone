using FitZone.Application.ServiceInterfaces;
using FitZone.Domain.Entities.Models;
using FitZone.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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
        public string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, user.Id.ToString()),
                new( ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role.RoleName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));


            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); /// ეს token ხელი მოაწერე ამ key-ით და გამოიყენე HMAC-SHA256.

            var token= new JwtSecurityToken(
                issuer:_jwtSettings.Issuer,
                audience:_jwtSettings.Audience,
                claims:claims,
                expires:DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials:credentials);

            return new JwtSecurityTokenHandler().WriteToken(token); // tokens მივეცით სპეციალური სამ ნაწილიანი ფორმა, header, oayder, signature და ისე დავაბრუნეთ.

        }

        public string GenerateRefreshToken()
        {
            throw new NotImplementedException();
        }
    }
}
