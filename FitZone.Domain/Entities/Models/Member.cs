using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.Entities.Models
{
    public class Member : User
    {
        public string? PhoneNumber { get; private set; }

    }
}
