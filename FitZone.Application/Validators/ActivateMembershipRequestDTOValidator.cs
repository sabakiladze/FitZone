using FitZone.Application.DTOs.MembershipDTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.Validators
{
    public  class ActivateMembershipRequestDTOValidator:AbstractValidator<ActivateMembershipRequestDTO>
    {
        public ActivateMembershipRequestDTOValidator()
        {
            RuleFor(x => x.PlanId).NotEqual(Guid.Empty).NotEmpty().
            WithMessage("Membership Id is not valid");

            RuleFor(x => x.Amount).GreaterThan(0m).WithMessage("Invalid Amount");


        }
       
    }
}
