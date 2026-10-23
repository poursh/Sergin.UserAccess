using FluentValidation;
using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.Create;
using Sergin.UserAccess.Domain.Roles;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Application.Roles.Commands.Create;

internal sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator(IScopeRepository scopes, IRoleQueryRepository roles, PermissionCatalog catalog)
    {
        RuleFor(x => x.OwnerScopeId).NotEmpty();

        RuleFor(x => x.OwnerScopeId)
            .MustAsync((id, cancellationToken) => scopes.ExistsAsync(new ScopeId(id), cancellationToken))
            .WithMessage("'Owner Scope Id' must refer to an existing Scope.")
            .When(x => x.OwnerScopeId != Guid.Empty);

        RuleFor(x => x.Name.Value)
            .NotEmpty()
            .MaximumLength(RoleName.MaxLength)
            .OverridePropertyName(nameof(CreateRoleCommand.Name));

        RuleFor(x => x.Permissions).NotNull();

        RuleFor(x => x.Permissions)
            .Must(permissions => permissions.All(permission => catalog.TryGet(permission, out _)))
            .WithMessage("'{PropertyName}' must be permissions the platform defines.")
            .When(x => x.Permissions is not null);

        // BR-4. Advisory: ix_roles_owner_scope_id_name guarantees the owner-scope part under a race.
        RuleFor(x => x.Name)
            .MustAsync(async (command, name, cancellationToken) =>
                !await roles.IsNameTakenInLineAsync(command.OwnerScopeId, name.Value, excludeRoleId: null, cancellationToken))
            .WithMessage("'Name' is already used by a role in this scope, one above it or one below it.")
            .When(x => x.OwnerScopeId != Guid.Empty && !string.IsNullOrWhiteSpace(x.Name.Value));
    }
}
