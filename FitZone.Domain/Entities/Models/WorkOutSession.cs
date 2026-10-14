using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.Entities.Models
{
   
        public class WorkoutSession
        {
            public Guid Id { get;  set; }= Guid.NewGuid();

        public DateTime StartsAt { get; set; }
            public DateTime EndsAt { get;  set; }

            public int Capacity { get;  set; }

            public Guid TrainerId { get;  set; }
            public Trainer? Trainer { get;  set; }


            public ICollection<Booking> Bookings { get;  set; }
                = new List<Booking>();
        }
   }
