using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public  class AlreadyBookedException:AppException
    {
        public AlreadyBookedException() : base("This user already has booked this WorkoutSession", 409) { }
    }
}
