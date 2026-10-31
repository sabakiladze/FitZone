using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public  class ForbiddenException:AppException
    {
        public ForbiddenException() : base("You are not allowed", 403) { }
    }
}
