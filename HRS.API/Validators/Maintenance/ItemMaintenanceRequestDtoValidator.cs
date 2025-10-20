using FluentValidation;
using HRS.Shared.Core.Dtos;

namespace HRS.API.Validators.Maintenance;

public class ItemMaintenanceRequestDtoValidator : AbstractValidator<FixItemMaintenanceRequestDto>
{
    public ItemMaintenanceRequestDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.QuantityFixed)
            .GreaterThan(0).WithMessage("QuantityFixed must be greater than zero.");

        RuleFor(x => x.Remarks)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
    }
}
