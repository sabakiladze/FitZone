using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class UserNameIsAlreadyTaken:Exception
    {
        public UserNameIsAlreadyTaken() : base("Username is alreadytaken") { }
        public UserNameIsAlreadyTaken(string message) : base(message) { }
        public UserNameIsAlreadyTaken(string message, Exception inner) : base(message, inner) { }


    }
}
