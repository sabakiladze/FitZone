using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage ="Email is required")]
        [StringLength(256, ErrorMessage ="Email must be less than 256 characters")]
        [EmailAddress(ErrorMessage ="Please enter a valid email address")]
        public string Email { get; set; }=string.Empty;


        [Required(ErrorMessage ="Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage ="Password must be between 8 and 100 characters")]
        public string Password { get; set; }=string.Empty;
    }
}
