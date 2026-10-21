using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class AccountIsDeactivatedException:AppException
    {
        public AccountIsDeactivatedException() : base("Account is deactivated.", 403) { }

    }
}
