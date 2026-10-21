using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class InvalidCredentialException:Exception
    {
        public InvalidCredentialException() : base("Invalid Password") { }
        public InvalidCredentialException(string message) : base(message) { }
        public InvalidCredentialException(string message, Exception inner) : base(message, inner) { }



    }
}
