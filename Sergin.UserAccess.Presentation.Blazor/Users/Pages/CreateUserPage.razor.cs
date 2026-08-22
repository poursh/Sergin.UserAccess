using Microsoft.AspNetCore.Components;
using Sergin.SharedKernel.Application.Dispatching;
using Sergin.SharedKernel.Presentation.Blazor.Errors;
using Sergin.UserAccess.Application.Users.Commands.Create;
using Sergin.UserAccess.Domain.Users;
using Sergin.UserAccess.Presentation.Blazor.Users.Models;

namespace Sergin.UserAccess.Presentation.Blazor.Users.Pages;

public sealed partial class CreateUserPage
{
    private readonly NewUserFormModel model = new();

    private bool submitting;

    [Inject]
    private ISerginSender Sender { get; set; } = default!;

    [Inject]
    private IUiErrorPresenter ErrorPresenter { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private async Task SubmitAsync()
    {
        submitting = true;

        ErrorOr<CreateUserCommandResponse> result =
            await Sender.SendAsync(new CreateUserCommand(new UserName(model.UserName)));

        submitting = false;

        if (result.IsError)
        {
            ErrorPresenter.Notify(result.FirstError);

            return;
        }

        Navigation.NavigateTo($"/ua/users/{result.Value.Id}");
    }
}
