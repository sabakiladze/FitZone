using FitZone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.Entities.Models
{
    public class Booking
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public User? User { get; set; } = null!;

        public Guid WorkoutSessionId { get; set; }
        public WorkoutSession? WorkoutSession { get; set; } = null!;

        public DateTime BookedAt { get; set; }
        public BookingStatus? Status { get; set; } 
    }
}
