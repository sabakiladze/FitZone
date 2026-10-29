using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class SessionHasBookingsException:AppException
    {
        public SessionHasBookingsException() : base("Session Has Bookings you can't delete it", 409) { }
    }
}
