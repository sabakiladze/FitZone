using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class BookingAlreadyCancelledException:AppException
    {
        public BookingAlreadyCancelledException() : base("Booking is already cancelled", 409) { }
    }
}
