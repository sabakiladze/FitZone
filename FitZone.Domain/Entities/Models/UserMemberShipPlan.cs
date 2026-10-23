using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.Entities.Models
{
    public class UserMembershipPlan
    {
        // მომხარებლების და წევრობის გეგმების ურთიერთობის მოდელი

        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;


        public Guid MembershipPlanId { get; set; }
        public MembershipPlan MembershipPlan { get; set; } = null!;

        public ICollection<Visit> Visits { get; set; } = new List<Visit>();


        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        public int? RemainingVisits { get; set; }

    }
    // with user 1:1 to many relationship, one user can have many membership plans, but one membership plan can only belong to one user.
}
