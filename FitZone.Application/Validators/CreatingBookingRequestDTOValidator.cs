using FitZone.Application.DTOs.BookingDTOs;
using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.Validators
{
    public  class CreatingBookingRequestDTOValidator:AbstractValidator<CreateBookingRequestDTO>
    {
        public CreatingBookingRequestDTOValidator()
        {
            RuleFor(x => x.WorkoutSessionId).NotEmpty().WithMessage("WorkoutSessionId is required.");
        }
    }
}
