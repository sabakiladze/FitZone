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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Infrastructure.JwtTokenService
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        public JwtTokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }
        

        public (string Token, DateTime ExpiresAt) GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, user.Id.ToString()),
                new( ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role?.RoleName ?? string.Empty)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            /*იღებს საიდუმლო ტექსტურ გასაღებს (Secret Key) კონფიგურაციიდან,
             * გადაიყვანს მას ბაიტების მასივში (Encoding.UTF8.GetBytes) 
             * და ქმნის სიმეტრიულ უსაფრთხოების გასაღებს (SymmetricSecurityKey).

            სიმეტრიული ნიშნავს, რომ ზუსტად ერთი და იგივე გასაღები გამოიყენება როგორც ტოკენის შესაქმნელად (ხელმოსაწერად),
            ისე მისი შემდგომი გადამოწმებისთვის (ვალიდაციისთვის).*/




            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            /*აერთიანებს ზემოთ შექმნილ გასაღებს კონკრეტულ კრიპტოგრაფიულ ჰეშირების ალგორითმთან — HmacSha256.
              შედეგად ვიღებთ ხელმოწერის სრულყოფილ მონაცემებს(SigningCredentials),
            რომლებიც შემდეგ გადაეცემა JWT ტოკენის გენერატორს, რათა ტოკენს უსაფრთხოების "ბეჭედი" დაუსვას*/

            var experesAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);


            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: experesAt,
                signingCredentials: credentials);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);


            return (accessToken,experesAt);
        }

        

        (string Token, string Hash, DateTime ExpiresAt) IJwtTokenService.GenerateRefreshToken()
        {
            //  Convert.tobase64stinf ეს 64 ბაიტი ერთიანდება ერთ სტრინგად. 
            // RandomNumberGenerator.GetBytes  ეს ცალ-ცალკე 64 ბაიტს აგენერირებს ად არა მთლიან strings

            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            //ბაზაში ტოკენის ჰეშირებულ ვარიანტს ჩავსვამ, ამიტომ მჭირდება დაჰაშვა 
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

            var experesAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

            return (refreshToken, hash, experesAt);
        }
    }
}
