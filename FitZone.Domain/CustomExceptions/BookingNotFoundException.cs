using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class BookingNotFoundException:AppException
    {
        public BookingNotFoundException() : base("Booking not found", 404) { }
    }
}
