using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.Auth
{
    public class LogOutRequet
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
