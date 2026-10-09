using Sergin.UserAccess.Application.Authorization;
using Sergin.UserAccess.Domain.Authorization;
using Sergin.UserAccess.Infrastructure.Data;

namespace Sergin.UserAccess.Infrastructure.Authorization;

internal sealed class AuthorizationAuditLog(IUserAccessDbContext dbContext) : IAuthorizationAuditLog
{
    public void Append(AuthorizationAuditEntry entry) => dbContext.Set<AuthorizationAuditEntry>().Add(entry);
}
