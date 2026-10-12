using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.Entities.Models
{
    public class Trainer:User
    {
        public string Specialization { get; private set; } = string.Empty;

        public int ExperienceYears { get; private set; }

    }
}
