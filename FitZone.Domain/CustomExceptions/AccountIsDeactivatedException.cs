using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class AccountIsDeactivatedException:Exception
    {
        public AccountIsDeactivatedException() : base("Account is deactivated.") { }
        public AccountIsDeactivatedException(string message) : base(message) { }
        public AccountIsDeactivatedException(string message, Exception inner) : base(message, inner) { }

    }
}
