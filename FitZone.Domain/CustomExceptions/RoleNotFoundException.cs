using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class RoleNotFoundException:Exception
    {
        public RoleNotFoundException() :base ("Role By This Name Do Could Not Found") { }
        public RoleNotFoundException(string message) : base(message) { }
        public RoleNotFoundException(string message, Exception Inner) : base(message, Inner) { }
    }
}
