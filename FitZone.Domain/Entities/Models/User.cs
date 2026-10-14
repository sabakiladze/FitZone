using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.Entities.Models
{
    public abstract class User
    {


        public Guid Id { get; set; }

        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }

        public bool IsEmailVerified { get; set; }

        public string? VerificationCode { get; set; }
        public DateTime? VerificationCodeExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }


        // როდესაც მომინდება ვერიფიკაცია ამ მეთოდით კოდს და დროს შევუცვლი მნიშვნელობას ერთდროულად და ცალ-ცალკე აღარ შევუცვლი მნიშვენლობას 
        public void SetVerificationCode(string code, DateTime expiresAt)
        {
            VerificationCode = code;
            VerificationCodeExpiresAt = expiresAt;
        }

        public void VerifyEmail()
        {
            IsEmailVerified = true;
             VerificationCode = null;
             VerificationCodeExpiresAt = null;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }

    }
}
