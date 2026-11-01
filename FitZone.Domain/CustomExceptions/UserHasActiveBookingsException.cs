using FitZone.Domain.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class UserHasActiveBookingsException:AppException
    {
        public UserHasActiveBookingsException() : base("You have upcoming bookings.Cancel them before deleting your account.", 409) { }
    }
}
