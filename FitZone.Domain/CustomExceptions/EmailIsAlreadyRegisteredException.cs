using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class EmailIsAlreadyRegisteredException:AppException
    {

        // base ში რაც იწერება ის მშობელს გადეცემა
        public EmailIsAlreadyRegisteredException() : base("Email is already registeres.", 409) { }

    }
}
