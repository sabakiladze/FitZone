using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class UserNameIsAlreadyTaken:AppException
    {
        public UserNameIsAlreadyTaken() : base("Username is alreadytaken.", 409) { }

    }
}
