using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.TrainerDTOs
{
    public class CreateTrainerRequestDTO // es administvis
    {
        [Required(ErrorMessage="FullName for Trainer is Required")]
        [StringLength(100, ErrorMessage ="Max 100 charachters for fullname")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage ="Specialization is rewuired")]
        [StringLength(500,ErrorMessage ="Max 500 characters for Specialization field")]
        public string Specialization { get; set; } = string.Empty;

        [Range(0, 60, ErrorMessage="'Experience Years' can be between 0 and 60 years")]
        public int ExperienceYears { get; set; }
    }
}
