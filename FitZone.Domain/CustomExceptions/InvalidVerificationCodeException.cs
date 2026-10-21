using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class InvalidVerificationCodeException:AppException
    {
        public InvalidVerificationCodeException() : base("Invalid Verification Code.",400){ }
    }
}
