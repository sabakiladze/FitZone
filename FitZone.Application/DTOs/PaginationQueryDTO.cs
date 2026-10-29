using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs
{
    public  class PaginationQueryDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page must be 1 or greater")]
        public int Page { get; set; } = 1;

        [Range(1, 50, ErrorMessage = "PageSize must be between 1 and 50")]
        public int PageSize { get; set; } = 10;
    }
}
