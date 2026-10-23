using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.MembershipDTOs
{
    public class ActivateMembershipRequestDTO
    {
        [Required(ErrorMessage ="Plan's id is required")]
        public Guid PlanId { get; set; }
    }
}
