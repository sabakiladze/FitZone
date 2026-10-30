using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public  class NotEnoyghMoneyException:AppException
    {
        public NotEnoyghMoneyException() : base($"Amount must be exactly.", 400) { }
    }
}
