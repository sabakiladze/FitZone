using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class SessionHasAlreadyStartedException:AppException
    {
        public SessionHasAlreadyStartedException() : base("Session has already started", 400) { }
    }
}
