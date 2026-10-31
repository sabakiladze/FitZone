using FluentValidation;
using FitZone.Application.DTOs.TrainerDTOs;

namespace FitZone.Application.Validators
{
    public  class CreateTrainerRequestDTOValidator:AbstractValidator<CreateTrainerRequestDTO>
    {
        public CreateTrainerRequestDTOValidator()
        {
            RuleFor(x => x.FullName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("FullName is required.")
            .MaximumLength(100).WithMessage("FullName must be at most 100 characters.")
            .MinimumLength(2).WithMessage("FullName must be at least 2 characters.");

            RuleFor(x => x.Specialization)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Specialization is required.")
                .MaximumLength(500).WithMessage("Specialization must be at most 500 characters.");

            RuleFor(x => x.ExperienceYears)
                .InclusiveBetween(0, 100).WithMessage("ExperienceYears must be between 0 and 100.");
        }
    }
}

