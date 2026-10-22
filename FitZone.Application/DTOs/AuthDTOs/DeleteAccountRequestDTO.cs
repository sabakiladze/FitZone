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
        [Required(ErrorMessage ="To delete your account first confirm by entering your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
