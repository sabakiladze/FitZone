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

        public Guid? UserId { get; set; }
        public User? User { get; set; } = null!;

        public Guid? UserMembershipPlanId { get; set; }
        public UserMembershipPlan? UserMembershipPlan { get; set; } = null!;

        public DateTime CheckedInAt { get; set; }

        public ICollection<Visit> Visits { get; set; } = new List<Visit>();

    }
    // with user 1:1 to many relationship, one user can have many visits, but one visit can only belong to one user.
}
