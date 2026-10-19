using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public  class EmailOrPasswordIsIncorrectException:Exception
    {
        public EmailOrPasswordIsIncorrectException() : base("Email Or Password Is Incorrect Exception") { }
        public EmailOrPasswordIsIncorrectException(string message) : base(message) { }

        public EmailOrPasswordIsIncorrectException(string message, Exception iner) : base(message, iner) { }

    }
}
