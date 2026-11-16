using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.CustomExceptions
{
    public  class AlreadyBookedException:AppException
    {

        // base ითგადაეცემა მშობელ კლასს წერილი და კოდი, appexception მიიიღებს ამ ორ მონაცმენს
        // კოდს თავის ველში შეინახავს, ხოლო წერილს გადასცემს Exception კლასს.
        public AlreadyBookedException() : base("This user already has booked this WorkoutSession", 409) { }
    }
}
