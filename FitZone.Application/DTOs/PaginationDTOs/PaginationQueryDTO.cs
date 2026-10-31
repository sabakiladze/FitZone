using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.PaginationDTOs
{
    public  class PaginationQueryDTO
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
