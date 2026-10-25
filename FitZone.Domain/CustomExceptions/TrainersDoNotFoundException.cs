using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class TrainersDoNotFoundException:AppException
    {
        public TrainersDoNotFoundException() : base("Trainers Do Not Found",400 ) { }
    }
}
