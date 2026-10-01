using Microsoft.AspNetCore.Components;
using MudBlazor;
using Sergin.SharedKernel.Presentation.Blazor.Errors;
using Sergin.UserAccess.Application.Contracts.Users.Commands.Create;
using Sergin.UserAccess.Domain.Users;
using Sergin.UserAccess.Presentation.Blazor.Users.Models;

namespace Sergin.UserAccess.Presentation.Blazor.Users.Pages;

public sealed partial class CreateUserPage
{
    private readonly NewUserFormModel model = new();

    private MudForm form = default!;
    private bool isValid;
    private bool submitting;
    private Func<object, string, Task<IEnumerable<string>>> validation = default!;

    private IReadOnlyList<SerginBreadcrumb> Trail { get; } =
    [
        SerginBreadcrumb.Of(UserAccessNavigation.Users),
        new("New user"),
    ];

    [Inject]
    private ISerginDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private ISerginFormValidator FormValidator { get; set; } = default!;

    [Inject]
    private IUiErrorPresenter ErrorPresenter { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    // An EventCallback, not a bare delegate: splatted onto MudForm's <form> it gets a receiver, so the
    // page re-renders after SubmitAsync.
    private EventCallback OnSubmit => EventCallback.Factory.Create(this, SubmitAsync);

    protected override void OnInitialized() => validation = FormValidator.RulesFor(ToCommand);

    // One mapping for both the field-by-field validation and the submit, so the two cannot drift.
    private CreateUserCommand ToCommand() => new(new UserName(model.UserName));

    private async Task SubmitAsync()
    {
        await form.ValidateAsync();

        if (!form.IsValid)
        {
            return;
        }

        submitting = true;

        ErrorOr<CreateUserCommandResponse> result = await Dispatcher.SendAsync(ToCommand());

        submitting = false;

        if (result.IsError)
        {
            // Every error, not the first: validation yields one per broken rule.
            ErrorPresenter.Notify(result.Errors);

            return;
        }

        Navigation.NavigateTo($"/ua/users/{result.Value.Id}");
    }
}
