using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.Entities.Models
{
    public class Trainer
    {

        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = null!;
        public string Specialization { get; set; } = null!;
        public int ExperienceYears { get; set; }
        public ICollection<WorkoutSession> WorkoutSessions { get; set; } = new List<WorkoutSession>();


    }
}
