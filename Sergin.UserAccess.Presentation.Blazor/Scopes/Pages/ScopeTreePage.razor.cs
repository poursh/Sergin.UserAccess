using Microsoft.AspNetCore.Components;
using Sergin.SharedKernel.Presentation.Blazor.Errors;
using Sergin.SharedKernel.Presentation.Errors;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.GetTree;

namespace Sergin.UserAccess.Presentation.Blazor.Scopes.Pages;

public sealed partial class ScopeTreePage
{
    private IReadOnlyCollection<ScopeTreeItem> scopes = [];
    private SerginProblem? problem;

    [Inject]
    private ISerginDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IUiErrorPresenter ErrorPresenter { get; set; } = default!;

    private static IReadOnlyList<SerginBreadcrumb> Trail { get; } = [SerginBreadcrumb.Of(UserAccessNavigation.Scopes)];

    protected override async Task OnInitializedAsync()
    {
        ErrorOr<IReadOnlyCollection<ScopeTreeItem>> result = await Dispatcher.SendAsync(new GetScopeTreeQueryCommand());

        if (result.IsError)
        {
            problem = ErrorPresenter.Present(result.FirstError);

            return;
        }

        scopes = result.Value;
    }
}
