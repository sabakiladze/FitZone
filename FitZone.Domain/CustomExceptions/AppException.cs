using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public abstract  class AppException:Exception
    {
        public int StatusCode { get; }

        // base ნიშნავს რომ გადასცემს მშობელ კლასს ამ შეტყობინებას.
        protected AppException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
