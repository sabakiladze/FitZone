using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class EmailIsAlreadyRegisteredException:Exception
    {
        public int StatusCode { get; }
        public EmailIsAlreadyRegisteredException() : base("Email is already registered") { }
        public EmailIsAlreadyRegisteredException(string message) : base( message) { }
        public EmailIsAlreadyRegisteredException(string message,Exception Inner) : base(message, Inner) { }


    }
}
