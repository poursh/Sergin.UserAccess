using FluentValidation;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.Delete;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Application.Scopes.Commands.Delete;

internal sealed class DeleteScopeCommandValidator : AbstractValidator<DeleteScopeCommand>
{
    public DeleteScopeCommandValidator(IScopeRepository scopes, IScopeQueryRepository scopeQueries)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .NotEqual(Scope.RootId.Value).WithMessage("The root scope cannot be deleted.");

        RuleFor(x => x.Id)
            .MustAsync(async (id, cancellationToken) => !await scopes.HasChildrenAsync(new ScopeId(id), cancellationToken))
            .WithMessage("A scope with child scopes cannot be deleted; delete or move its children first.")
            .When(x => x.Id != Guid.Empty);

        // BR-5: in use means live assignments at it or live roles owned by it; only an explicit cascade proceeds.
        RuleFor(x => x.Cascade)
            .MustAsync(async (command, _, cancellationToken) =>
                !await scopeQueries.HasLiveAssignmentsAsync(command.Id, cancellationToken)
                && !await scopeQueries.OwnsLiveRolesAsync(command.Id, cancellationToken))
            .WithMessage("The scope has active assignments or owns roles; delete it with cascade to revoke and delete them too.")
            .When(x => !x.Cascade && x.Id != Guid.Empty);
    }
}
