using Microsoft.AspNetCore.Components;
using MudBlazor;
using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.SharedKernel.Domain.Securities;
using Sergin.SharedKernel.Presentation.Blazor.Errors;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.Create;
using Sergin.UserAccess.Domain.Roles;
using Sergin.UserAccess.Presentation.Blazor.Roles.Models;

namespace Sergin.UserAccess.Presentation.Blazor.Roles.Pages;

public sealed partial class CreateRolePage
{
    private readonly NewRoleFormModel model = new();

    private IReadOnlyCollection<string> knownPermissions = [];
    private MudForm form = default!;
    private bool isValid;
    private bool submitting;
    private Func<object, string, Task<IEnumerable<string>>> validation = default!;

    [SupplyParameterFromQuery]
    public Guid ScopeId { get; set; }

    [Inject]
    private ISerginDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private ISerginFormValidator FormValidator { get; set; } = default!;

    [Inject]
    private IUiErrorPresenter ErrorPresenter { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    // The permission catalogue: every permission a command configuration requires. The server still refuses
    // one the user does not hold in this scope (the permission ceiling).
    [Inject]
    private CommandConfigurationRegistry Configurations { get; set; } = default!;

    private EventCallback OnSubmit => EventCallback.Factory.Create(this, SubmitAsync);

    private IReadOnlyList<SerginBreadcrumb> Trail =>
    [
        SerginBreadcrumb.Of(UserAccessNavigation.Scopes),
        new("Scope", $"/ua/scopes/{ScopeId}"),
        new("New role"),
    ];

    protected override void OnInitialized()
    {
        validation = FormValidator.RulesFor(ToCommand);
        knownPermissions = [.. Configurations.KnownPermissions.Select(permission => permission.Value)];
    }

    private CreateRoleCommand ToCommand() => new(
        ScopeId,
        new RoleName(model.Name),
        model.IsInheritable,
        [.. model.Permissions.Select(Permission.Create)]);

    private async Task SubmitAsync()
    {
        await form.ValidateAsync();

        if (!form.IsValid)
        {
            return;
        }

        submitting = true;

        ErrorOr<CreateRoleCommandResponse> result = await Dispatcher.SendAsync(ToCommand());

        submitting = false;

        if (result.IsError)
        {
            ErrorPresenter.Notify(result.Errors);

            return;
        }

        Navigation.NavigateTo($"/ua/roles/{result.Value.Id}");
    }
}
