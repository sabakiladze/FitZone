using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class EmailIsNotVerifiedException:AppException
    {
        public EmailIsNotVerifiedException() : base( "Email is Not Veirfied Yet, Please Verify Account First",403) { }
    }
}
