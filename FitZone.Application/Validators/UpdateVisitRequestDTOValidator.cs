using FitZone.Application.DTOs.VisitDTOs;
using FluentValidation;

namespace FitZone.Application.Validators
{
    public class UpdateVisitRequestDTOValidator : AbstractValidator<UpdateVisitRequestDTO>
    {
        public UpdateVisitRequestDTOValidator()
        {
            RuleFor(x => x.CheckedInAt)
                .NotEmpty().WithMessage("CheckedInAt is required.")
                .Must(d => d.ToUniversalTime() <= DateTime.UtcNow)
                .WithMessage("CheckedInAt cannot be in the future.");
        }
    }
}
