using Microsoft.AspNetCore.Components;
using Sergin.SharedKernel.Application.Dispatching;
using Sergin.SharedKernel.Presentation.Blazor.Errors;
using Sergin.SharedKernel.Presentation.Errors;
using Sergin.UserAccess.Application.Users.Commands.DeactivateUser;
using Sergin.UserAccess.Application.Users.Commands.GetOne;

namespace Sergin.UserAccess.Presentation.Blazor.Users.Pages;

public sealed partial class UserDetailPage
{
    private UserQueryResponse? user;
    private SerginProblem? problem;
    private bool deactivating;

    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    private ISerginSender Sender { get; set; } = default!;

    [Inject]
    private IUiErrorPresenter ErrorPresenter { get; set; } = default!;

    protected override async Task OnParametersSetAsync()
    {
        ErrorOr<UserQueryResponse> result = await Sender.SendAsync(new GetUserByIdQueryCommand(Id));

        if (result.IsError)
        {
            user = null;
            problem = ErrorPresenter.Present(result.FirstError);

            return;
        }

        problem = null;
        user = result.Value;
    }

    private async Task DeactivateAsync()
    {
        deactivating = true;

        ErrorOr<DeactivateUserCommandResponse> result = await Sender.SendAsync(new DeactivateUserCommand(Id));

        deactivating = false;

        if (result.IsError)
        {
            ErrorPresenter.Notify(result.FirstError);
        }
    }
}
