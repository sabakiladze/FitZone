using FitZone.Domain.CustomExceptions;
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


        public int? RemainingVisits { get; private set; }

        public UserMembershipPlan(Guid userId, Guid planId, DateTime start, int durationInDays, int? visitLimit)
        {
            UserId = userId;
            MembershipPlanId = planId;
            StartDate = start;
            EndDate = start.AddDays(durationInDays);
            RemainingVisits = visitLimit;

            // რადგან ომმიწირა რომ RemailingVisits გამეხადა private set,
            // ამიტომ კონსტრუქტორიდან შევცვლი და მოვანიჭებ მნიშვნელობას
        }


        // ცარიელი კონსტრუქტორი მჭირდება ენტიტი ფრამეწორკისთვის
        public UserMembershipPlan() { }

        public void MakeVisit()
        {
            if (RemainingVisits is null) return;

            if (RemainingVisits <= 0)
                throw new NoVisitsLeftException();

            RemainingVisits--;
        }


    }
    // with user 1:1 to many relationship, one user can have many membership plans, but one membership plan can only belong to one user.
}
