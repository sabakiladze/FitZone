using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.MembershipDTOs
{
    public class CreateMembershipPlanRequestDTO
    {
        public string Name { get; set; } = string.Empty;
        public int DurationInDays { get; set; }
        public int? VisitLimit { get; set; }      
        public decimal Price { get; set; }
    }
}
