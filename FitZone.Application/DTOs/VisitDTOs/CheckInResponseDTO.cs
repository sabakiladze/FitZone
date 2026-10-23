using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.VisitDTOs
{
    public class CheckInResponseDTO
    {
        public Guid VisitId { get; set; }
        public DateTime CheckedInAt { get; set; }
        public int? RemainingVisits { get; set; }
    }
}
