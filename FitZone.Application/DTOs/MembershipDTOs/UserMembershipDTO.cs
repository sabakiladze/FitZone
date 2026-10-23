using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.MembershipDTOs
{
    public class UserMembershipDTO
    {

        // ამას გამოვიყენებ ჩემი აბინიმენტის სანახავად.. usermembershipplan

        public Guid Id { get; set; }
        public string PlanName { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? RemainingVisits { get; set; }     
        public bool IsActive { get; set; }
    }
}
