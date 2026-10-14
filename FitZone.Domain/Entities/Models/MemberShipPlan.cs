using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.Entities.Models
{
    public  class MemberShipPlan
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Decimal Price { get; set; }
        public int VisitLimit { get; set; }
        public int DurationInDays { get; set; }

    }
}
