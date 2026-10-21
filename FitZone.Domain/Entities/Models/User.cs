using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.Entities.Models
{
    public  class User
    {


        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserName { get; set; }= null!;
        public string FirstName { get; set; }= null!;
        public string LastName { get; set; }= null!;
        public string Email { get; set; }= null!;
        public string PasswordHash { get; set; } = null!;

        public bool IsEmailVerified { get; private set; } = false;
        public string? VerificationCode { get; set; }
        public DateTime? VerificationCodeExpiresAt { get; set; }

        public string? RefreshTokenHash { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        public DateTime? DeletedAt { get; private set; }

        public Guid? RoleId { get; set; } = null!;
        public Role? Role { get; set; }= null!;



        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<UserMembershipPlan> UserMembershipPlans { get; set; } = new List<UserMembershipPlan>();
        public ICollection<Visit> Visits { get; set; } = new List<Visit>();





        // როდესაც მომინდება ვერიფიკაცია ამ მეთოდით კოდს და დროს შევუცვლი მნიშვნელობას ერთდროულად და ცალ-ცალკე აღარ შევუცვლი მნიშვენლობას 
        public void SetVerificationCode(string codeHash, DateTime expiresAt)
        {
            VerificationCode = codeHash;              // ბაზაში ჰეში ინახება და არა კოდი
            VerificationCodeExpiresAt = expiresAt;
        }
        public void VerifyEmail()
        {
            IsEmailVerified = true;
             VerificationCode = null;
             VerificationCodeExpiresAt = null;
        }


        public void MarkAsDeleted()
        {
            var suffix = Id.ToString("N");                    

            IsActive = false;
            DeletedAt = DateTime.UtcNow;

            UserName = $"deleted_{suffix}";                   
            Email = $"deleted_{suffix}@deleted.local";
            FirstName = "Deleted";
            LastName = "User";

            PasswordHash = Guid.NewGuid().ToString("N");      
            ClearRefreshToken();
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

        public void SetRefreshToken(string hash, DateTime expiresAt)
        {
            RefreshTokenHash = hash;
            RefreshTokenExpiresAt = expiresAt;
        }

        public void ClearRefreshToken()
        {
            RefreshTokenHash = null;
            RefreshTokenExpiresAt = null;
        }

    }
}
