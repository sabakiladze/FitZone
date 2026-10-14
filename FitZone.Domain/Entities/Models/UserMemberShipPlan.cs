using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.Entities.Models
{
    public class UserMemberShipPlan
    {
        // მომხარებლების და წევრობის გეგმების ურთიერთობის მოდელი

        public Guid Id { get; set; }


        public Guid? MemberId { get; set; }
        public Member? Member{ get; set; }


        public Guid? MemberShipPlanId { get; set; }
        public MemberShipPlan? MemberShipPlan { get; set; }


        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int RemainingVisits { get; set; }

    }
}
