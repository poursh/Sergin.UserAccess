---
name: add-feature
description: Scaffold a new CQRS vertical slice (command or query) for the UserAccess module — Contracts records, Application handler and validator, request configuration, Infrastructure repository wiring, Presentation endpoint, optional Blazor page, and DI/route registration. Invoke with /add-feature.
disable-model-invocation: false
---

Scaffold a new vertical-slice feature for: $ARGUMENTS

Expected input: `<AggregateFolder>[/<ChildEntityFolder>] <FeatureName> <command|query>`, e.g. `/add-feature Users DeactivateUser command` or `/add-feature Users/Assignments GetEffective query`. Ask the user for whatever is missing before generating anything — don't guess the aggregate or verb.

This repo has no scaffolding CLI; slices are hand-authored following a strict, repeated shape. Use `Users/**` (the `Users` aggregate) as the reference implementation for every file below — read the matching file there before writing the new one, and match its style exactly (sealed records, primary constructors, `ErrorOr<T>` returns, no comments). `Roles/**` and `Scopes/**` are the references for a soft-deletable, versioned root and for a permission checked at the record's own scope.

This repo is embed-only (see `README.md`): it is edited from inside a host that mounts it as a submodule (today Sergin.MeterMinder, at `src/Modules/UserAccess/`). Paths below are relative to **this repo's own root**. The host's `/add-feature` skill (`.claude/skills/add-feature/SKILL.md` in Sergin.MeterMinder) holds the full rules for list queries, aggregate features and the UI; this file covers what is specific to UserAccess.

## Layout to create (aggregate = e.g. `Users`, feature = e.g. `DeactivateUser`)

**Command** (state-changing):
1. `Sergin.UserAccess.Application.Contracts/<Aggregate>/Commands/<Feature>/<Feature>Command.cs` — `public sealed record <Feature>Command(...) : ICommand<<Feature>CommandResponse>;`. The records live in `.Application.Contracts`, never in `.Application`.
2. `.../<Feature>/<Feature>CommandResponse.cs` (same project) — `public sealed record <Feature>CommandResponse(...);`
3. `Sergin.UserAccess.Application/<Aggregate>/Commands/<Feature>/<Feature>CommandHandler.cs` — `internal sealed class` implementing `ICommandHandler<TCommand, TResponse>`, primary-constructor-injects `IUserAccessUnitOfWork` + the domain repository, calls a domain factory/behavior method, calls `unitOfWork.SaveChangesAsync`, returns the response.
   - **Every write that changes a grant** — a scope, a role, an assignment, or a user's active state — also injects `IAuthorizationCacheInvalidator` and calls `cache.Invalidate()` after the save. Without it a revoked grant keeps passing checks until the cache expires. `DeactivateUserCommandHandler` is the reference.
   - **A permission checked at the record's own scope** (a role at its owner scope, a scope at itself) cannot be expressed in a configuration. Inject `IScopeAuthorization`, load the record, and return `Error.Forbidden()` when `authorization.CurrentUserHasAsync(<Aggregate>Permissions.X, scopeId, cancellationToken)` is false. `RenameRoleCommandHandler` is the reference.
   - **Acting on a user** also checks that every live assignment of that user is inside the caller's reach (`UserReachGuard`), so a regional administrator never acts on the root administrator.
4. `.../<Feature>/<Feature>CommandValidator.cs` (same `.Application` folder) — every write command has one: `internal sealed class <Feature>CommandValidator : AbstractValidator<<Feature>Command>` with `using FluentValidation;`. Rules on a value object target `.Value` and keep `.OverridePropertyName(nameof(<Feature>Command.X))`; a repository rule (`MustExistIn`, `MustBeUniqueIn` from `Sergin.SharedKernel.Application.Validations`) targets the wrapper. Length limits come from `const int MaxLength` on the value object. Nothing to register. Reference: `Users/Commands/Create/CreateUserCommandValidator.cs`.
5. If the domain aggregate needs a new factory method or behavior (e.g. `User.Deactivate()`), add it to the aggregate class in `Sergin.UserAccess.Domain`. Don't add public setters — mutate via methods on the aggregate. A pure domain rule gets a test in the host's `tests/Sergin.UserAccess.UnitTests` (`.Domain` grants it `InternalsVisibleTo`).
6. Presentation: `Sergin.UserAccess.Presentation.WebApi/<Aggregate>/Endpoints/<Feature>/<Feature>Endpoint.cs` implementing `IEndpoint.MapEndpoint` (`internal class`, never sealed), mapping the appropriate HTTP verb, binding a request model (add one alongside the endpoint if the command needs a body, e.g. `New<X>Model.cs`), sending via MediatR's `ISender` directly — `ErrorOr<TResponse> res = await sender.Send(new <Feature>Command(...));` — returning `res.ToApiResult()`. The route string has no schema segment (`/users`, not `/ua/users`). No API host runs it today; write it anyway.
7. Register the endpoint in `Sergin.UserAccess/<Aggregate>/<Aggregate>InstallationExtensions.Map<Aggregate>Endpoints` (e.g. `UserInstallationExtensions.MapUserEndpoints`) — instantiate and call `.MapEndpoint(routeBuilder)`. For a brand-new aggregate, create that file first (copy `UserInstallationExtensions.cs`) and wire it into `UserAccessModule`: `services.Add<Aggregate>Dependencies()` in `AddServices` and `group.Map<Aggregate>Endpoints()` in `MapEndpoints`.
8. If a new repository interface/dependency is needed, register it in the same file's `Add<Aggregate>Dependencies` (`services.AddTransient<IFoo, Foo>()`, one call per interface).

**Query** (read-side, bypasses EF):
Same shape but under `Commands/<Feature>/` still (queries live in the `Commands` folder alongside commands — match that, don't invent a `Queries` folder), implementing `IQuery<TResponse>` / `IQueryHandler<TQuery, TResponse>` from `Sergin.SharedKernel.Application.Commands.Queries`. The records go in `.Application.Contracts`; the handler and the `I<Feature>QueryRepository` interface stay in `.Application`. The repository returns a nullable response and the handler maps null to `Error.NotFound()`. Implement it in `Sergin.UserAccess.Infrastructure/<Aggregate>/Repositories/Queries/<Aggregate>QueryRepository.cs` using `IDbConnectionFactory` + raw SQL against the `ua` schema (see `UserQueryRepository.cs`) — never use EF Core for reads. `Role` and `Scope` are soft-deletable: every read of `ua.roles` or `ua.scopes` adds `deleted_at_utc IS NULL`. A GetOne over a versioned root (`Role`, `Scope`) derives from `VersionedQueryHandler<TQuery, TResponse>`.

**Request policy.** A request that needs a permission or an expected version gets `Sergin.UserAccess.Application.Configurations/<Aggregate>[/<ChildEntity>]/Commands/<Feature>/<RecordName>Configuration.cs`, bound to its aggregate root. The permission is a `PermissionDefinition` from `.Application.Configurations/<Aggregate>/<Aggregate>Permissions.cs` (`PermissionDefinition.Create(name, label)`), registered with `builder.Permissions(...)` in the root's `<Root>AggregateFeatureConfiguration`; a new aggregate defines its own class. `RequireExpectedVersion()` is allowed only on a versioned root (`Role`, `Scope`). Never put a configuration in `.Application.Contracts` or `.Application`: only `ConfigurationsAssembly` is read, and host start refuses one anywhere else. `ProvisionExternalUserCommand` must never get one — it runs while the caller is still anonymous. Template:

```csharp
using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Users.Commands.DeactivateUser;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Application.Configurations.Users.Commands.DeactivateUser;

internal sealed class DeactivateUserCommandConfiguration : ICommandConfiguration<DeactivateUserCommand, User>
{
    public void Configure(CommandConfigurationBuilder<DeactivateUserCommand> builder) =>
        builder.RequirePermissions(UserPermissions.Manage);
}
```

## Optional: the UI slice

Ask whether the feature needs a Blazor page. UserAccess pages are **not yet on the view split** DeviceManagement uses: each page under `Sergin.UserAccess.Presentation.Blazor/<Aggregate>/Pages/` owns its content, renders `<SerginBreadcrumbs Trail="Trail" />` after its `<PageTitle>`, and writes the `MudForm` shape by hand. Copy `Users/Pages/CreateUserPage`, `UserDetailPage` or `UserListPage` until the module migrates. The rules that do not change with the split — inject `ISerginDispatcher`, never `ISender`; no `@code` blocks; the `/ua/` route prefix; `ErrorPresenter.Notify`/`Present`; form models with no DataAnnotations — are in the host's `/add-feature` skill and root CLAUDE.md. A new list page gets a named `SerginNavItem` on `UserAccessNavigation`, composed into `Items`, with the list query's permission as `RequiredPermission`.

## After scaffolding

1. Check each new project's `GlobalUsings.cs` (and `_Imports.razor` in `.Presentation.Blazor`) before adding `using` statements — many namespaces (`ErrorOr`, `Sergin.SharedKernel.*`) are already global.
2. If the feature needs new/changed columns, add or update the `IEntityTypeConfiguration` in `Sergin.UserAccess.Infrastructure.Data`, then generate a migration **from the host repo's root** (this repo has no host of its own):
   ```
   dotnet ef migrations add <Name> --project src/Modules/UserAccess/Sergin.UserAccess.Infrastructure.Data --startup-project src/Hosts/Sergin.MeterMinder.Hosts.All
   ```
   Convert the scaffolded `<Timestamp>_<Name>.cs` to a file-scoped namespace by hand before building (IDE0161 is an error); leave the `.Designer.cs` and the snapshot alone. Paths are for the Sergin.MeterMinder layout.
3. Build **from the host's root** — every analyzer/style warning is an error:
   ```
   dotnet build Sergin.MeterMinder.slnx
   ```
4. Add tests in the host: an integration test under `tests/Sergin.MeterMinder.IntegrationTests.All` (`Authorization/` for an RBAC rule, `RbacScenario` builds a scope tree), a row in `Commands/ModuleCommandConfigurationTests` for a new configuration, and a page route in `Shell/ModulePageRenderingTests` for a new page.
5. Commit inside `src/Modules/UserAccess/` (this repo), then bump the submodule pointer in the host.
