using FitZone.Domain.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.ServiceInterfaces
{
    public interface IJwtTokenService
    {
      
            (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);
            (string Token, string Hash, DateTime ExpiresAt) GenerateRefreshToken();
        
    }
}
