using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class UserNotFoundException:AppException
    {
        public UserNotFoundException() : base("User not found.",404){ }

    }
}
