using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.BookingDTOs
{
    public class CreateBookingRequestDTO
    {
        [Required(ErrorMessage ="To book a session you must neter wotkoutSessionId")]
        public Guid WorkoutSessionId { get; set; }
    }
}
