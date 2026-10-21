using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class InvalidVerificationCodeException:Exception
    {
        public InvalidVerificationCodeException() : base("Invalid Verification Code"){ }
        public InvalidVerificationCodeException(string message) : base(message) { }
        public InvalidVerificationCodeException(string message, Exception inner) : base(message, inner) { }

    }
}
