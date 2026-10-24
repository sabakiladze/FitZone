using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.TrainerDTOs
{
    public class TrainerDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Specialization { get; set; } = null!;
        public int ExperienceYears { get; set; }
    }
}
