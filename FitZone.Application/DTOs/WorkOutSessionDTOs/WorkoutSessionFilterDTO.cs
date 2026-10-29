using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.WorkOutSessionDTOs
{
    public class WorkoutSessionFilterDTO
    {
        public Guid? TrainerId {  get; set; }
        public DateOnly? Date {  get; set; }
    }
}
