using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.VisitDTOs
{
    public  class VisitDTO
    {
        public Guid Id { get; set; }
        public DateTime CheckedInAt { get; set; }
        public string PlanName { get; set; } = null!;

    }
}
