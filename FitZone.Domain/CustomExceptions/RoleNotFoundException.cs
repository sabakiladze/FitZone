using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class RoleNotFoundException:AppException
    {
        public RoleNotFoundException() :base ("Role By This Name Could Not Found.", 500) { }
    }
}
