using FitZone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.BookingDTOs
{
    public class BookingDTO
    {
        public Guid Id { get; set; }
        public Guid WorkoutSessionId { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime BookedAt { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public string TrainerName { get; set; } = null!;
    }
}
