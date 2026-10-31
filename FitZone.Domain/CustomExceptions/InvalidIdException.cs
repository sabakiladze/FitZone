using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class InvalidIdException:AppException
    {
        public InvalidIdException() : base("Invalid Id.", 400) { }
    }
}
