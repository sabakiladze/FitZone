using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.AuthDTOs
{
    public class SentVerificationCodeDTO
    {
        [Required(ErrorMessage="Email is Required.")]
        public string Email { get; set; } = null!;
        
    }
}
