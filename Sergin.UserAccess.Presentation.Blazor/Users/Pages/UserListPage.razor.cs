using Microsoft.AspNetCore.Components;
using MudBlazor;
using Sergin.SharedKernel.Presentation.Blazor.Dispatching;
using Sergin.SharedKernel.Presentation.Blazor.Errors;
using Sergin.UserAccess.Application.Users.Commands.GetList;

namespace Sergin.UserAccess.Presentation.Blazor.Users.Pages;

public sealed partial class UserListPage
{
    [Inject]
    private ISerginDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IUiErrorPresenter ErrorPresenter { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private async Task<TableData<GetUserListItem>> LoadAsync(TableState state, CancellationToken cancellationToken)
    {
        // MudBlazor's TableState.Page is 0-based; Sergin's PageIndex is 1-based.
        ErrorOr<ListQueryResponse<GetUserListItem>> result =
            await Dispatcher.SendListAsync<GetUserListItem>(state.PageSize, state.Page + 1, cancellationToken);

        if (result.IsError)
        {
            ErrorPresenter.Notify(result.FirstError);

            return new TableData<GetUserListItem> { Items = [], TotalItems = 0 };
        }

        return new TableData<GetUserListItem> { Items = result.Value.Data, TotalItems = result.Value.Total };
    }

    private void OpenUser(GetUserListItem? item)
    {
        if (item is not null)
        {
            Navigation.NavigateTo($"/ua/users/{item.Id}");
        }
    }
}
