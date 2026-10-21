using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class VerificationCodeExpiredException:Exception
    {
        public VerificationCodeExpiredException() : base("Verification code Expired") { }
        public VerificationCodeExpiredException(string message) : base(message) { }
        public VerificationCodeExpiredException(string message, Exception inner) : base(message, inner) { }

    }
}
}
