using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public class TrainerHasUpcomingSessionsException:AppException
    {
        public TrainerHasUpcomingSessionsException() : base("Trainer has upcoming sessions. Delete or reassign them first.", 409) { }
    }
}
