namespace Sergin.UserAccess.Presentation.WebApi.Users.Assignments.Endpoints.Assign;

public record NewAssignmentModel(Guid RoleId, Guid ScopeId, bool IncludeSubtree, DateTime? ValidFromUtc, DateTime? ValidUntilUtc);
