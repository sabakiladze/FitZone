using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class MembershipPlanNameAlreadyExistsException : AppException
    {
        public MembershipPlanNameAlreadyExistsException()
            : base("A membership plan with this name already exists.", 409) { }
    }

}
