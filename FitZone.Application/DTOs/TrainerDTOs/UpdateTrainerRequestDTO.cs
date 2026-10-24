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
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Specialization is required.")]
        [StringLength(500, ErrorMessage = "Specialization cannot exceed 500 characters.")]
        public string Specialization { get; set; } = null!;

        [Range(0, 60, ErrorMessage = "Experience years must be between 0 and 60.")]
        public int ExperienceYears { get; set; }
    }
}
