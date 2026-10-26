using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.WorkOutSessionDTOs
{
   public class WorkoutSessionDTO
    {
        public Guid Id { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }

        public int Capacity { get; set; }

        public Guid TrainerId { get; set; }
    }
}
