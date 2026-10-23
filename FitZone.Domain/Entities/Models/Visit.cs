using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.Entities.Models
{
    public class Visit
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CheckedInAt { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid UserMembershipPlanId { get; set; }
        public UserMembershipPlan UserMembershipPlan { get; set; } = null!;
        

    }
}
