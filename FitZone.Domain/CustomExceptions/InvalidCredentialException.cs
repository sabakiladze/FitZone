using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class InvalidCredentialException : AppException
    {
        public InvalidCredentialException() : base("Invalid Credential", 401) { }



    }
}
