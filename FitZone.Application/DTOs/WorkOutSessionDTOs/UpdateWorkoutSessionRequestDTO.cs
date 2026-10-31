using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.WorkOutSessionDTOs
{
    public class UpdateWorkoutSessionRequestDTO
    {
        public Guid  Id { get; set; }
        public Guid TrainerId { get; set; }

        public DateTime StartsAt { get; set; }

        public DateTime EndsAt { get; set; }

        public int Capacity { get; set; }
    }
}
