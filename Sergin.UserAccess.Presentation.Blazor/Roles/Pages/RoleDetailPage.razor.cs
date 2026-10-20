using Microsoft.AspNetCore.Components;
using MudBlazor;
using Sergin.SharedKernel.Application.Concurrency;
using Sergin.SharedKernel.Application.Localizations;
using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.SharedKernel.Domain;
using Sergin.SharedKernel.Domain.Securities;
using Sergin.SharedKernel.Presentation.Blazor.Errors;
using Sergin.SharedKernel.Presentation.Errors;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.Delete;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.GetOne;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.GrantPermission;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.Rename;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.RevokePermission;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.SetInheritable;
using Sergin.UserAccess.Domain.Roles;
using Sergin.UserAccess.Presentation.Blazor.Roles.Models;

namespace Sergin.UserAccess.Presentation.Blazor.Roles.Pages;

public sealed partial class RoleDetailPage
{
    private readonly RoleNameFormModel model = new();

    private RoleQueryResponse? role;
    private RowVersion? version;
    private SerginProblem? problem;
    private string? permissionToGrant;
    private bool cascade;
    private bool busy;

    private MudForm form = default!;
    private bool isValid;
    private Func<object, string, Task<IEnumerable<string>>> validation = default!;

    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    private ISerginDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private ISerginFormValidator FormValidator { get; set; } = default!;

    [Inject]
    private IUiErrorPresenter ErrorPresenter { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private PermissionCatalog Catalog { get; set; } = default!;

    [Inject]
    private ILocalizer Localizer { get; set; } = default!;

    private EventCallback OnRename => EventCallback.Factory.Create(this, RenameAsync);

    private IReadOnlyList<SerginBreadcrumb> Trail =>
    [
        SerginBreadcrumb.Of(UserAccessNavigation.Scopes),
        new(role?.OwnerScopeName ?? "Scope", role is null ? null : $"/ua/scopes/{role.OwnerScopeId}"),
        new(role?.Name ?? "Role"),
    ];

    // The role's grants the catalogue defines, shown by label; a grant it no longer defines is listed apart by its
    // raw name, still revocable.
    private IEnumerable<PermissionDefinition> GrantedDefined =>
        (role?.Permissions ?? []).Select(TryDefine).OfType<PermissionDefinition>();

    private IReadOnlyCollection<string> GrantedUnknown =>
        [.. (role?.Permissions ?? []).Where(permission => TryDefine(permission) is null)];

    protected override void OnInitialized() => validation = FormValidator.RulesFor(ToRenameCommand);

    private string LabelOf(string? name) => name is null ? string.Empty : PermissionLabels.Of(Catalog, Localizer, name);

    // A stored row that is not even a well-formed name is unknown too, not a render failure.
    private PermissionDefinition? TryDefine(string permission) =>
        Permission.TryCreate(permission, out Permission? parsed) && Catalog.TryGet(parsed, out PermissionDefinition? definition)
            ? definition
            : null;

    private bool IsGranted(PermissionDefinition definition) =>
        role?.Permissions.Contains(definition.Permission.Value) == true;

    protected override Task OnParametersSetAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        ErrorOr<Versioned<RoleQueryResponse>> loaded = await Dispatcher.SendVersionedAsync(new GetRoleByIdQueryCommand(Id));

        if (loaded.IsError)
        {
            role = null;
            version = null;
            problem = ErrorPresenter.Present(loaded.FirstError);

            return;
        }

        problem = null;
        role = loaded.Value.Value;
        version = loaded.Value.Version;
        model.Name = role.Name;
    }

    private RenameRoleCommand ToRenameCommand() => new(Id, new RoleName(model.Name));

    private async Task RenameAsync()
    {
        await form.ValidateAsync();

        if (form.IsValid)
        {
            await SendGuardedAsync(ToRenameCommand());
        }
    }

    private Task SetInheritableAsync(bool isInheritable) => SendGuardedAsync(new SetRoleInheritableCommand(Id, isInheritable));

    private Task GrantAsync() =>
        permissionToGrant is null ? Task.CompletedTask : SendGuardedAsync(new GrantRolePermissionCommand(Id, Permission.Create(permissionToGrant)));

    private Task RevokeAsync(string permission) => SendGuardedAsync(new RevokeRolePermissionCommand(Id, Permission.Create(permission)));

    // Every edit is sent at the version this page loaded; the page reloads afterwards either way, so a stale
    // answer leaves it showing the current role.
    private async Task SendGuardedAsync<TResponse>(IRequest<ErrorOr<TResponse>> command)
    {
        if (version is not { } expected)
        {
            return;
        }

        busy = true;
        ErrorOr<Versioned<TResponse>> result = await Dispatcher.SendVersionedAsync(command, expected);
        busy = false;

        if (result.IsError)
        {
            ErrorPresenter.Notify(result.Errors);
        }

        permissionToGrant = null;
        await LoadAsync();
    }

    private async Task DeleteAsync()
    {
        if (version is not { } expected || role is null)
        {
            return;
        }

        Guid ownerScopeId = role.OwnerScopeId;

        busy = true;
        ErrorOr<Versioned<Deleted>> result = await Dispatcher.SendVersionedAsync(new DeleteRoleCommand(Id, cascade), expected);
        busy = false;

        if (result.IsError)
        {
            ErrorPresenter.Notify(result.Errors);
            await LoadAsync();

            return;
        }

        Navigation.NavigateTo($"/ua/scopes/{ownerScopeId}");
    }
}
