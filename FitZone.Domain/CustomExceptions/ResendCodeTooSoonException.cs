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
            : base("Please wait until your current verification code expires before requesting a new one.", 429) { }
    }
}
