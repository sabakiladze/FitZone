using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class YouAlreadyHavePlanException:AppException
    {
        public YouAlreadyHavePlanException() : base("You already have a membership plan.", 409) { }
    }
}
