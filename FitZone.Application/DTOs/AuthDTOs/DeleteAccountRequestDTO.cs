using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.AuthDTOs
{
    public class DeleteAccountRequestDTO
    {
        [Required]
        public string Password { get; set; } = null!;
    }
}
