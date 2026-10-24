using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class NoActiveMembershipException:AppException
    {
        public NoActiveMembershipException() : base("You do not have any active membership plan.", 402) { }
    }
}
