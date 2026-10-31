using FitZone.Application.DTOs.MembershipDTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.Validators
{
   public class UpdateMembershipPlanRequestDTOValidator: AbstractValidator<UpdateMembershipPlanRequestDTO>
    {
        public UpdateMembershipPlanRequestDTOValidator()
        {
            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Membership plan name is required.")
                .MinimumLength(2).WithMessage("Membership plan name must be at least 2 characters.")
                .MaximumLength(100).WithMessage("Membership plan name must be at most 100 characters.");

            RuleFor(x => x.DurationInDays)
                .InclusiveBetween(1, 365)
                .WithMessage("DurationInDays must be between 1 and 365.");

            RuleFor(x => x.VisitLimit)
                .GreaterThan(0).When(x => x.VisitLimit.HasValue)
                .WithMessage("VisitLimit must be greater than 0, or null for unlimited.");

            RuleFor(x => x.Price)
                .GreaterThan(0m)
                .WithMessage("Price must be greater than 0.");

        }
    }
}
