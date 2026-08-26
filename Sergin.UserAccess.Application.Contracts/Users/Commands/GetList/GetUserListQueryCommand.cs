using Sergin.SharedKernel.Application.Commands.Queries;
using Sergin.SharedKernel.Application.Securities.Authorization;

namespace Sergin.UserAccess.Application.Users.Commands.GetList;

[RequiredPermissions("permission.ua.users.read")]
public sealed record GetUserListQueryCommand : ListQuery<GetUserListItem>
{
    public GetUserListQueryCommand(
        Paggination paggination,
        Term? term = default,
        Filtering? filtering = default,
        Sorting? sorting = default)
        : base(paggination, term, filtering, sorting)
    {
    }
}
