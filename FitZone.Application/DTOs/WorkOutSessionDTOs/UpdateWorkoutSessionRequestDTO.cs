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
        [Required(ErrorMessage ="Id is neccessery to Update WorkoutSession")]
        public Guid  Id { get; set; }
        [Required(ErrorMessage = "TrainerId is required")]
        public Guid TrainerId { get; set; }

        [Required(ErrorMessage = "StartsAt is required")]
        public DateTime StartsAt { get; set; }

        [Required(ErrorMessage = "EndsAt is required")]
        public DateTime EndsAt { get; set; }

        [Range(1, 200, ErrorMessage = "Capacity must be between 1 and 200")]
        public int Capacity { get; set; }
    }
}
