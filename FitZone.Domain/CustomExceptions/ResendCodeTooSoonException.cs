using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class ResendCodeTooSoonException : AppException
    {
        public ResendCodeTooSoonException()
            : base("Please wait a minute before requesting a new verification code.", 429) { }
    }
}
