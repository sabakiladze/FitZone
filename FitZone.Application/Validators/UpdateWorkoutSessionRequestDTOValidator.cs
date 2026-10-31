using FitZone.Application.DTOs.WorkOutSessionDTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.Validators
{
    public class UpdateWorkoutSessionRequestDTOValidator : AbstractValidator<UpdateWorkoutSessionRequestDTO>
    {
        public UpdateWorkoutSessionRequestDTOValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");
            RuleFor(x => x.StartsAt).GreaterThan(x => DateTime.UtcNow)
                .WithMessage("Session must start in the future.");
            RuleFor(x => x.EndsAt).GreaterThan(x => x.StartsAt)
                .WithMessage("EndsAt must be later than StartsAt.");
            RuleFor(x => x.Capacity).InclusiveBetween(1, 200);
        }
    }
}
