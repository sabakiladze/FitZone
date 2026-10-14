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
        public User? User{ get; set; }


        public Guid MembershipPlanId { get; set; }
        public MembershipPlan? MembershipPlan { get; set; }


        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int RemainingVisits { get; set; }

    }
}
