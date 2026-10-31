using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class NoVisitsLeftException:AppException
    {
        public NoVisitsLeftException() : base("No Visits Left.", 403) { }
    }
}
