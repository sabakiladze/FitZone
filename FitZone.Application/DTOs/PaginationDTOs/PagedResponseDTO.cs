using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.DTOs.PaginationDTOs
{
    public class PagedResponseDTO<T>
    {
        public int TotalCount { get; set; }
        public ICollection<T> Values { get; set; }= new List<T>();
    }
}
