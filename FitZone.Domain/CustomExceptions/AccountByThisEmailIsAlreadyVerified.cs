using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class AccountByThisEmailIsAlreadyVerified : AppException
    {
        public AccountByThisEmailIsAlreadyVerified() : base("Account by this email is already verified.", 409) { }
    }
}
