using Sergin.UserAccess.Application.Users.Commands.GetList;
using Sergin.UserAccess.Application.Users.Commands.GetOne;
using Sergin.UserAccess.Application.Users.Commands.ProvisionExternalUser;

namespace Sergin.UserAccess.Application.Users;
public interface IUserAllQueryRepository
    : IGetUserListQueryRepository, IGetUserQueryRepository, IGetUserPermissionsQueryRepository;
