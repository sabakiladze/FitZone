using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class StatusNotValidException:AppException
    {
        public StatusNotValidException() : base("Status is not valid",400 ) { }
    }
}
