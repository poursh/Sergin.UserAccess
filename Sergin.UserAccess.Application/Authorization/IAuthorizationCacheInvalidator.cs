namespace Sergin.UserAccess.Application.Authorization;

/// <summary>
/// Drops every cached effective grant (NFR-3). A handler that changes a scope, a role, an assignment or a user's
/// active flag calls it once its save has committed — never before, or a check racing the save could cache the
/// old state again. In-process only: a second host instance keeps its own cache.
/// </summary>
public interface IAuthorizationCacheInvalidator
{
    void Invalidate();
}
