using FluentValidation;
using Lib.Service.Validators.Interfaces;

namespace Lib.Service.Validators;

public class EmailRequestValidator : AbstractValidator<IEmailRequestValidator>
{
    public EmailRequestValidator()
    {
        RuleFor(r => r.Email).NotEmpty().EmailAddress();
    }
}