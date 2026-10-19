using Microsoft.AspNetCore.Components;
using MudBlazor;
using Sergin.SharedKernel.Presentation.Blazor.Errors;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.Create;
using Sergin.UserAccess.Domain.Scopes;
using Sergin.UserAccess.Presentation.Blazor.Scopes.Models;

namespace Sergin.UserAccess.Presentation.Blazor.Scopes.Pages;

public sealed partial class CreateScopePage
{
    private readonly ScopeNameFormModel model = new();

    private MudForm form = default!;
    private bool isValid;
    private bool submitting;
    private Func<object, string, Task<IEnumerable<string>>> validation = default!;

    /// <summary>The scope the new one goes under; the root when the link names none.</summary>
    [SupplyParameterFromQuery]
    public Guid ParentId { get; set; }

    [Inject]
    private ISerginDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private ISerginFormValidator FormValidator { get; set; } = default!;

    [Inject]
    private IUiErrorPresenter ErrorPresenter { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private EventCallback OnSubmit => EventCallback.Factory.Create(this, SubmitAsync);

    private IReadOnlyList<SerginBreadcrumb> Trail { get; } =
    [
        SerginBreadcrumb.Of(UserAccessNavigation.Scopes),
        new("New scope"),
    ];

    protected override void OnInitialized() => validation = FormValidator.RulesFor(ToCommand);

    protected override void OnParametersSet()
    {
        if (ParentId == Guid.Empty)
        {
            ParentId = Scope.RootId.Value;
        }
    }

    private CreateScopeCommand ToCommand() => new(ParentId, new ScopeName(model.Name));

    private async Task SubmitAsync()
    {
        await form.ValidateAsync();

        if (!form.IsValid)
        {
            return;
        }

        submitting = true;

        ErrorOr<CreateScopeCommandResponse> result = await Dispatcher.SendAsync(ToCommand());

        submitting = false;

        if (result.IsError)
        {
            ErrorPresenter.Notify(result.Errors);

            return;
        }

        Navigation.NavigateTo($"/ua/scopes/{result.Value.Id}");
    }
}
