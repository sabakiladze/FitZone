using FitZone.Application.DTOs.PaginationDTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.Validators
{
   
    public class PaginationQueryDTOValidator : AbstractValidator<PaginationQueryDTO>
    {
        public PaginationQueryDTOValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("Page must be greater than 1");
            RuleFor(x => x.PageSize).InclusiveBetween(1, 50).WithMessage("page size must be between 1 and 50");
        }
    }
}
