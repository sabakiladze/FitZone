using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.Auth
{
    public class RegisterRequestDTO
    {
        [Required(ErrorMessage ="UserName is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage ="UserName must be between 3 and 50 characters")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage ="FirstName is required")]
        [StringLength(50, ErrorMessage ="FirstName must be less than 50 characters")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage ="LastName is required")]
        [StringLength(50, ErrorMessage ="LastName must be less than 50 characters")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage ="Email is required")]
        [EmailAddress(ErrorMessage ="Please enter a valid email address")]
        [StringLength(256, ErrorMessage ="Email must be less than 256 characters")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage ="Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage ="Password must be between 8 and 100 characters")]
        public string Password { get; set; } = null!;
    }
}
