using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public  class EmailOrPasswordIsIncorrectException:AppException
    {
        public EmailOrPasswordIsIncorrectException() : base("Email Or Password Is Incorrect.", 401) { }

    }
}
