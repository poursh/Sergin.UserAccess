namespace Sergin.UserAccess.Presentation.WebApi.Roles.Endpoints.Create;

public record NewRoleModel(Guid OwnerScopeId, string Name, bool IsInheritable, IReadOnlyCollection<string> Permissions);
