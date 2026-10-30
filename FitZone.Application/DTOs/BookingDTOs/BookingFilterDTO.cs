using FitZone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.BookingDTOs
{
    public class BookingFilterDTO
    {
        public Guid? WorkoutSessionId { get; set; }
        public string? TrainerName { get; set; }
        public DateTime? DateTime { get; set; }
        public BookingStatus? Status { get; set; }

    }
}
