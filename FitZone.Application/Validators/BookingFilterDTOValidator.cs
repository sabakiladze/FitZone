using FitZone.Application.DTOs.BookingDTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.Validators
{
    public class BookingFilterDTOValidator:AbstractValidator<BookingFilterDTO>
    {
        public BookingFilterDTOValidator()
        {
            RuleFor(x => x.WorkoutSessionId)
                .NotEqual(Guid.Empty).When(x => x.WorkoutSessionId.HasValue)
                .WithMessage("WorkoutSessionId is not valid.");

            RuleFor(x => x.Status)
                .IsInEnum().When(x => x.Status.HasValue)
                .WithMessage("Status is not valid.");

            RuleFor(x => x.TrainerName)
                .MaximumLength(100).WithMessage("TrainerName must be at most 100 characters.");
        }
    }
}
