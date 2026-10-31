using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.TrainerDTOs
{
    public class UpdateTrainerRequestDTO //administvis
    {
        public Guid Id { get; set; }    
        public string FullName { get; set; } = string.Empty;

        public string Specialization { get; set; } = string.Empty;

        public int ExperienceYears { get; set; }
    }
}
