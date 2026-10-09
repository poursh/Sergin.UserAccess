using Sergin.UserAccess.Domain.Authorization;

namespace Sergin.UserAccess.Application.Authorization;

/// <summary>Appends to the authorization audit log on the current save. There is no update or delete, by design.</summary>
public interface IAuthorizationAuditLog
{
    void Append(AuthorizationAuditEntry entry);
}
