using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class PageSizeException:AppException
    {
        public PageSizeException() : base("PageSize must be between 1 and 50", 400) { }
    }
}
