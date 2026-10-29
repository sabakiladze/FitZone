using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class WorkoutSessionNotFoundException :AppException
    {
        public WorkoutSessionNotFoundException() : base("WorkOutSession NotcFound", 404) { }
    }
}
