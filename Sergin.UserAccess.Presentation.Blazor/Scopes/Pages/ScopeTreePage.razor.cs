using System.Globalization;
using Microsoft.AspNetCore.Components;
using Sergin.SharedKernel.Presentation.Blazor.Errors;
using Sergin.SharedKernel.Presentation.Errors;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.GetTree;

namespace Sergin.UserAccess.Presentation.Blazor.Scopes.Pages;

public sealed partial class ScopeTreePage
{
    private List<ScopeTreeItem> ordered = [];
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

        ordered = DepthFirst(result.Value);
    }

    // Each scope directly under its parent, siblings by name: the tree read top to bottom.
    private static List<ScopeTreeItem> DepthFirst(IReadOnlyCollection<ScopeTreeItem> scopes)
    {
        ILookup<Guid?, ScopeTreeItem> children = scopes.ToLookup(scope => scope.ParentId);
        List<ScopeTreeItem> result = [];

        void Visit(Guid? parentId)
        {
            foreach (ScopeTreeItem scope in children[parentId].OrderBy(item => item.Name, StringComparer.CurrentCulture))
            {
                result.Add(scope);
                Visit(scope.Id);
            }
        }

        Visit(null);

        return result;
    }

    private string Indent(ScopeTreeItem scope)
    {
        int top = ordered.Count == 0 ? 1 : ordered.Min(item => item.Depth);

        return string.Create(CultureInfo.InvariantCulture, $"padding-left: {(scope.Depth - top) * 24 + 16}px");
    }
}
