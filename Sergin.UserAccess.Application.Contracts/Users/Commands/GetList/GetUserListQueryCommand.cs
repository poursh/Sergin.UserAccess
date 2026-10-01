using Sergin.SharedKernel.Application.Commands.Queries;

namespace Sergin.UserAccess.Application.Contracts.Users.Commands.GetList;

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
