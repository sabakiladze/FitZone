using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class BookingTimeConflictException:AppException
    {
        public BookingTimeConflictException() : base("User already has booked another session on this time." ,409) { }
    }
}
