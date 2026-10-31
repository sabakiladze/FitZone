using FitZone.Application.DTOs.WorkOutSessionDTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.Validators
{
    public class RemoveWorkoutSessionDTOValidator:AbstractValidator<RemoveWorkOutSessionDTO>
    {
        public RemoveWorkoutSessionDTOValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Workout Session id is required");
        }
    }
}
