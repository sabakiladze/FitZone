using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class WorkoutSessionNotFounfException :AppException
    {
        public WorkoutSessionNotFounfException() : base("WorkOutSession NotcFound", 404) { }
    }
}
