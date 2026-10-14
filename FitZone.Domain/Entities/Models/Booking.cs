using FitZone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FitZone.Domain.Entities.Models.WorkOutSession;
using static FitZone.Domain.Enums.BookingStatus;

namespace FitZone.Domain.Entities.Models
{
    public class Booking
    {
        public Guid Id { get; set; }

        public Guid MemberId { get; set; }
        public Member? Member { get; set; }

        public Guid WorkoutSessionId { get; set; }
        public WorkoutSession? WorkoutSession { get; set; }

        public DateTime BookedAt { get; set; }
        public BookingStatus? Status { get; set; }
    }
}
