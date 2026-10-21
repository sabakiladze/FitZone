using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class VerificationCodeExpiredException:AppException
    {
        public VerificationCodeExpiredException() : base("Verification code Expired", 400) { }

    }
}

