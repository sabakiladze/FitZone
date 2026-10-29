using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class SessionTimeConflictException : AppException
    {
        public SessionTimeConflictException()
            : base("The trainer already has a session at this time.", 409) { }
    }
}
