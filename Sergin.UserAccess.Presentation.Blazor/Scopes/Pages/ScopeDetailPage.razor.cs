using Microsoft.AspNetCore.Components;
using MudBlazor;
using Sergin.SharedKernel.Application.Concurrency;
using Sergin.SharedKernel.Domain;
using Sergin.SharedKernel.Presentation.Blazor.Errors;
using Sergin.SharedKernel.Presentation.Errors;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.GetAssignable;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.Delete;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.GetOne;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.GetTree;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.Move;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.Rename;
using Sergin.UserAccess.Domain.Scopes;
using Sergin.UserAccess.Presentation.Blazor.Scopes.Models;

namespace Sergin.UserAccess.Presentation.Blazor.Scopes.Pages;

public sealed partial class ScopeDetailPage
{
    private readonly ScopeNameFormModel model = new();

    private ScopeQueryResponse? scope;
    private RowVersion? version;
    private SerginProblem? problem;
    private IReadOnlyCollection<AssignableRoleItem> roles = [];
    private IReadOnlyCollection<ScopeTreeItem> moveTargets = [];
    private Guid newParentId;
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

    private EventCallback OnRename => EventCallback.Factory.Create(this, RenameAsync);

    private IReadOnlyList<SerginBreadcrumb> Trail =>
    [
        SerginBreadcrumb.Of(UserAccessNavigation.Scopes),
        new(scope?.Name ?? "Scope"),
    ];

    protected override void OnInitialized() => validation = FormValidator.RulesFor(ToRenameCommand);

    protected override Task OnParametersSetAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        ErrorOr<Versioned<ScopeQueryResponse>> loaded = await Dispatcher.SendVersionedAsync(new GetScopeByIdQueryCommand(Id));

        if (loaded.IsError)
        {
            scope = null;
            version = null;
            problem = ErrorPresenter.Present(loaded.FirstError);

            return;
        }

        problem = null;
        scope = loaded.Value.Value;
        version = loaded.Value.Version;
        model.Name = scope.Name;

        ErrorOr<IReadOnlyCollection<AssignableRoleItem>> assignable = await Dispatcher.SendAsync(new GetAssignableRolesQueryCommand(Id));
        roles = assignable.IsError ? [] : assignable.Value;

        // Anywhere but here and below: the server refuses a cycle too, this just keeps it out of the list.
        ErrorOr<IReadOnlyCollection<ScopeTreeItem>> tree = await Dispatcher.SendAsync(new GetScopeTreeQueryCommand());
        moveTargets = tree.IsError ? [] : [.. tree.Value.Where(candidate => !IsSelfOrBelow(tree.Value, candidate))];
    }

    private bool IsSelfOrBelow(IReadOnlyCollection<ScopeTreeItem> tree, ScopeTreeItem candidate)
    {
        ScopeTreeItem? current = candidate;

        while (current is not null)
        {
            if (current.Id == Id)
            {
                return true;
            }

            current = tree.FirstOrDefault(item => item.Id == current.ParentId);
        }

        return false;
    }

    private RenameScopeCommand ToRenameCommand() => new(Id, new ScopeName(model.Name));

    private async Task RenameAsync()
    {
        await form.ValidateAsync();

        if (!form.IsValid || version is not { } expected)
        {
            return;
        }

        busy = true;
        ErrorOr<Versioned<RenameScopeCommandResponse>> result = await Dispatcher.SendVersionedAsync(ToRenameCommand(), expected);
        busy = false;

        if (result.IsError)
        {
            ErrorPresenter.Notify(result.Errors);
        }

        await LoadAsync();
    }

    private async Task MoveAsync()
    {
        busy = true;
        ErrorOr<MoveScopeCommandResponse> result = await Dispatcher.SendAsync(new MoveScopeCommand(Id, newParentId));
        busy = false;

        if (result.IsError)
        {
            ErrorPresenter.Notify(result.Errors);

            return;
        }

        await LoadAsync();
    }

    private async Task DeleteAsync()
    {
        busy = true;
        ErrorOr<Deleted> result = await Dispatcher.SendAsync(new DeleteScopeCommand(Id, cascade));
        busy = false;

        if (result.IsError)
        {
            ErrorPresenter.Notify(result.Errors);

            return;
        }

        Navigation.NavigateTo(scope?.ParentId is { } parentId ? $"/ua/scopes/{parentId}" : "/ua/scopes");
    }
}
