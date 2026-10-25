using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class BadRequestException:AppException
    {
        public BadRequestException(string info) : base(info,400) { }
    }
}
