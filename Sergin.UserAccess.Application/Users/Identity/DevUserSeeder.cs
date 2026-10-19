using Sergin.SharedKernel.Application.Securities.Users;
using Sergin.UserAccess.Application.Authorization;
using Sergin.UserAccess.Domain.Roles;
using Sergin.UserAccess.Domain.Scopes;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Application.Users.Identity;

/// <summary>
/// Gives the configured development user a row under its configured id and the seeded administrator role at the
/// root over the whole tree. Idempotent: an existing user keeps its row, and an existing grant is not repeated.
/// </summary>
/// <remarks>
/// Writes through the repositories rather than a command: it runs at start, outside any caller, and a command
/// would need a permission no caller holds yet.
/// </remarks>
internal sealed class DevUserSeeder(
    IUserAccessUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IAuthorizationCacheInvalidator cache) : IDevUserSeeder
{
    public async Task EnsureAsync(DevUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        Role administrator = await roleRepository.GetByName(Scope.RootId, new RoleName(Role.AdministratorRoleName), cancellationToken)
            ?? throw new InvalidOperationException(
                $"The '{Role.AdministratorRoleName}' role owned by the root scope is missing, so the development user cannot be seeded.");

        var id = new UserInternalId(user.Id);
        User? existing = await userRepository.GetWithAssignmentsAsync(id, cancellationToken);

        if (existing is null)
        {
            var userName = new UserName(user.UserName);

            if (await userRepository.IsTakenAsync(userName, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Sergin:DevUser:UserName '{user.UserName}' belongs to another user, so the development user cannot be seeded under id {user.Id}.");
            }

            existing = User.CreateDevelopmentUser(
                id,
                userName,
                string.IsNullOrWhiteSpace(user.Email) ? null : new EmailAddress(user.Email),
                user.FirstName,
                user.LastName);

            userRepository.Insert(existing);
        }

        existing.AssignGlobalRole(administrator);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        cache.Invalidate();
    }
}
