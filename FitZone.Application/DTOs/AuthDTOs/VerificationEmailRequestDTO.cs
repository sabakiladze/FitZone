using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.AuthDTOs
{
    public class VerificationEmailRequestDTO
    {
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = null!;

        [Required, StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } =  null!;
    }
}
