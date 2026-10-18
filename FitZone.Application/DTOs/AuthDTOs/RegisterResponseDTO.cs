using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.AuthDTOs
{
    public class RegisterResponseDTO
    {
        public Guid UserId { get; set; }
        public required string Email { get; set; }
    }
}
