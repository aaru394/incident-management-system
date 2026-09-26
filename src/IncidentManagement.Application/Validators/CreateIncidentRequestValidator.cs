using FluentValidation;
using IncidentManagement.Application.DTOs;

namespace IncidentManagement.Application.Validators;

public class CreateIncidentRequestValidator : AbstractValidator<CreateIncidentRequest>
{
    public CreateIncidentRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Severity).IsInEnum();
    }
}
