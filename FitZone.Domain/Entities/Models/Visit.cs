using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.Entities.Models
{
    public class Visit
    {
        public Guid Id { get; set; }
        
        public Guid? MemberId { get; set; }
        public Member? Member { get; set; }

        public Guid? UserMemberShipPlanId { get; set; }
        public UserMemberShipPlan? UserMemberShipPlan { get; set; }

        public DateTime CheckedInAt { get; set; }
    }
}
