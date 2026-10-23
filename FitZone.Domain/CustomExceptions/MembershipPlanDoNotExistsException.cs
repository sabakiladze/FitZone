using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class MembershipPlanDoNotExistsException:AppException
    {
        public MembershipPlanDoNotExistsException() : base("Membership plan by this id do not exists.", 404) { }
    }
}
