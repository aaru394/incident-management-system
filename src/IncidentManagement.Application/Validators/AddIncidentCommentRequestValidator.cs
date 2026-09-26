using FluentValidation;
using IncidentManagement.Application.DTOs;

namespace IncidentManagement.Application.Validators;

public class AddIncidentCommentRequestValidator : AbstractValidator<AddIncidentCommentRequest>
{
    public AddIncidentCommentRequestValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}
