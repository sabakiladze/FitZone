using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.WorkOutSessionDTOs
{
    public  class RemoveWorkOutSessionDTO
    {
        [Required(ErrorMessage ="To delete WorkoutSession Id is required")]
        public Guid Id { get; set; }
    }
}
