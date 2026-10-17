using System.Globalization;
using Microsoft.AspNetCore.Components;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.GetTree;

namespace Sergin.UserAccess.Presentation.Blazor.Scopes.Components;

/// <summary>
/// Scopes as an indented tree, each name linking to its detail page. With <see cref="UnderId"/> it shows only the
/// scopes below that one.
/// </summary>
public sealed partial class ScopeTreeTable
{
    private List<ScopeTreeItem> ordered = [];
    private int topDepth;

    /// <summary>The flat tree, as <c>GetScopeTree</c> returns it.</summary>
    [Parameter]
    public IReadOnlyCollection<ScopeTreeItem> Scopes { get; set; } = [];

    /// <summary>The scope whose descendants to show; <see langword="null"/> shows the whole tree.</summary>
    [Parameter]
    public Guid? UnderId { get; set; }

    [Parameter]
    public string EmptyText { get; set; } = "No scopes.";

    protected override void OnParametersSet()
    {
        ordered = DepthFirst(Scopes, UnderId);
        topDepth = ordered.Count == 0 ? 0 : ordered.Min(item => item.Depth);
    }

    // Each scope directly under its parent, siblings by name: the tree read top to bottom.
    private static List<ScopeTreeItem> DepthFirst(IReadOnlyCollection<ScopeTreeItem> scopes, Guid? underId)
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

        Visit(underId);

        return result;
    }

    private string Indent(ScopeTreeItem scope) =>
        string.Create(CultureInfo.InvariantCulture, $"padding-left: {(scope.Depth - topDepth) * 24 + 16}px");
}
