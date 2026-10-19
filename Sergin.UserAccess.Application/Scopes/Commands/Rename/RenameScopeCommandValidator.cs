using FluentValidation;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.Rename;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Application.Scopes.Commands.Rename;

internal sealed class RenameScopeCommandValidator : AbstractValidator<RenameScopeCommand>
{
    public RenameScopeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name.Value)
            .NotEmpty()
            .MaximumLength(ScopeName.MaxLength)
            .OverridePropertyName(nameof(RenameScopeCommand.Name));
    }
}
