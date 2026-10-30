using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class SessionFullException:AppException
    {
        public SessionFullException() : base("WorkoutSession is full, you can't book this sesssion.", 409) { }
    }
}
