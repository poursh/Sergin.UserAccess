using Sergin.SharedKernel.Application.Commands;
using Sergin.SharedKernel.Application.Securities;
using Sergin.UserAccess.Domain.Roles;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Application.Users.Commands.ProvisionExternalUser;

internal sealed class ProvisionExternalUserCommandHandler(
    IUserAccessUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IGetUserPermissionsQueryRepository permissionsRepository)
    : ICommandHandler<ProvisionExternalUserCommand, ProvisionExternalUserCommandResponse>
{
    public async Task<ErrorOr<ProvisionExternalUserCommandResponse>> Handle(
        ProvisionExternalUserCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByExternalId(request.ExternalId, cancellationToken);

        if (user is null)
        {
            // A local user may predate external sign-in. Matching on the name links the two rather than
            // creating a second account for the same person.
            user = await userRepository.GetByUserName(request.UserName, cancellationToken);

            if (user is null)
            {
                user = User.CreateFromExternalIdentity(
                    request.ExternalId, request.UserName, request.Email, request.FirstName, request.LastName);

                await AssignDefaultRole(user, cancellationToken);

                userRepository.Insert(user);
            }
            else
            {
                user.LinkExternalId(request.ExternalId);
            }
        }

        user.UpdateProfile(request.Email, request.FirstName, request.LastName);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        IReadOnlyCollection<Permission> permissions =
            await permissionsRepository.GetPermissions(user.Id, cancellationToken);

        return new ProvisionExternalUserCommandResponse(user.Id.Value, permissions);
    }

    /// <summary>
    /// A brand-new user with no roles would sign in successfully and then see nothing, with no screen in
    /// the product to fix it. The default role is seeded by migration for exactly this.
    /// </summary>
    private async Task AssignDefaultRole(User user, CancellationToken cancellationToken)
    {
        Role? defaultRole = await roleRepository.GetByName(new RoleName(Role.DefaultRoleName), cancellationToken);

        if (defaultRole is not null)
        {
            user.AssignRole(defaultRole.Id);
        }
    }
}
