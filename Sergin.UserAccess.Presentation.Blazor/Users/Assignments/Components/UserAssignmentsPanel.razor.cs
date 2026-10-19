using System.Globalization;
using Microsoft.AspNetCore.Components;
using Sergin.SharedKernel.Presentation.Blazor.Errors;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.GetAssignable;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.GetTree;
using Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.Assign;
using Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.GetEffective;
using Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.GetList;
using Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.Revoke;
using Sergin.UserAccess.Presentation.Blazor.Users.Assignments.Models;

namespace Sergin.UserAccess.Presentation.Blazor.Users.Assignments.Components;

/// <summary>A user's assignments (FR-6, FR-12), the assign form (FR-7, FR-8, FR-10) and what they add up to (FR-11).</summary>
public sealed partial class UserAssignmentsPanel
{
    private NewAssignmentFormModel model = new();

    private IReadOnlyCollection<UserAssignmentItem> assignments = [];
    private IReadOnlyCollection<EffectivePermissionItem> effective = [];
    private IReadOnlyCollection<ScopeTreeItem> scopes = [];
    private IReadOnlyCollection<AssignableRoleItem> roles = [];
    private bool busy;

    [Parameter]
    public Guid UserId { get; set; }

    [Inject]
    private ISerginDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IUiErrorPresenter ErrorPresenter { get; set; } = default!;

    protected override async Task OnParametersSetAsync()
    {
        ErrorOr<IReadOnlyCollection<ScopeTreeItem>> tree = await Dispatcher.SendAsync(new GetScopeTreeQueryCommand());
        scopes = tree.IsError ? [] : [.. tree.Value.OrderBy(scope => scope.Depth).ThenBy(scope => scope.Name, StringComparer.CurrentCulture)];

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        ErrorOr<IReadOnlyCollection<UserAssignmentItem>> listed = await Dispatcher.SendAsync(new GetUserAssignmentsQueryCommand(UserId));
        ErrorOr<IReadOnlyCollection<EffectivePermissionItem>> held = await Dispatcher.SendAsync(new GetUserEffectivePermissionsQueryCommand(UserId));

        assignments = listed.IsError ? [] : listed.Value;
        effective = held.IsError ? [] : held.Value;
    }

    private async Task OnScopeChangedAsync(Guid scopeId)
    {
        model.ScopeId = scopeId;
        model.RoleId = Guid.Empty;

        ErrorOr<IReadOnlyCollection<AssignableRoleItem>> assignable = await Dispatcher.SendAsync(new GetAssignableRolesQueryCommand(scopeId));

        if (assignable.IsError)
        {
            roles = [];
            ErrorPresenter.Notify(assignable.FirstError);

            return;
        }

        roles = assignable.Value;
    }

    private async Task AssignAsync()
    {
        busy = true;

        ErrorOr<AssignRoleCommandResponse> result = await Dispatcher.SendAsync(new AssignRoleCommand(
            UserId,
            model.RoleId,
            model.ScopeId,
            model.IncludeSubtree,
            AsUtc(model.ValidFromUtc),
            AsUtc(model.ValidUntilUtc)));

        busy = false;

        if (result.IsError)
        {
            ErrorPresenter.Notify(result.Errors);

            return;
        }

        model = new NewAssignmentFormModel { ScopeId = model.ScopeId };
        await LoadAsync();
    }

    private async Task RevokeAsync(Guid assignmentId)
    {
        busy = true;
        ErrorOr<RevokeAssignmentCommandResponse> result = await Dispatcher.SendAsync(new RevokeAssignmentCommand(UserId, assignmentId));
        busy = false;

        if (result.IsError)
        {
            ErrorPresenter.Notify(result.FirstError);
        }

        await LoadAsync();
    }

    // A picked calendar day is that day's UTC midnight.
    private static DateTime? AsUtc(DateTime? day) => day is { } value ? DateTime.SpecifyKind(value.Date, DateTimeKind.Utc) : null;

    private static string Period(UserAssignmentItem assignment) => (assignment.ValidFromUtc, assignment.ValidUntilUtc) switch
    {
        (null, null) => "Always",
        ({ } from, null) => string.Create(CultureInfo.CurrentCulture, $"From {from:d}"),
        (null, { } until) => string.Create(CultureInfo.CurrentCulture, $"Until {until:d}"),
        ({ } from, { } until) => string.Create(CultureInfo.CurrentCulture, $"{from:d} to {until:d}"),
    };
}
