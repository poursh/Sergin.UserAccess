# Sergin.UserAccess

**The identity and access module of the Sergin platform — schema `ua`. It owns users, the scope tree, roles and role assignments, and it answers every permission check the other modules make: Keycloak proves who someone is, and `ua` decides what they may do, and where.**

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/)
[![Schema](https://img.shields.io/badge/schema-ua-336791)](Sergin.UserAccess.Infrastructure.Data)
[![Embed-only](https://img.shields.io/badge/builds-embed--only-B45309)](#this-repository-does-not-build-on-its-own)
[![License: MIT](https://img.shields.io/badge/License-MIT-2E7D32)](LICENSE)

Sergin is a .NET 10 modular monolith. Its host, [Sergin.MeterMinder](https://github.com/poursh/Sergin.MeterMinder), composes this module with **DeviceManagement**, a head-end system for smart electricity, gas and water meters. Every other module depends on this one for authorization.

## What it does

### Scoped RBAC

Authorization is **role-based and scoped**. Scopes form one tree (for example *Company › North Region › Feeder 1*), stored as a PostgreSQL `ltree` path. A role belongs to a scope and may be **inheritable**, which makes it assignable below its owner too. A user holds **assignments**: a role at a scope, for that scope only or for its whole subtree, with an optional validity window.

A permission check asks: *does this user hold permission P at scope S?* The answer comes from the stored grants on every check, cached in process and invalidated by every write that changes a scope, a role, an assignment or a user's active state. A grant change takes effect on the next request, with no new sign-in. Any failure during a check is a denial.

The module implements SharedKernel's authorization seams, so the rest of the platform never reads `ua` tables directly:

| SharedKernel seam | Implementation here | Used for |
|---|---|---|
| `IScopedPermissionChecker` | `ScopedPermissionChecker` | Every pipeline, nav and handler permission check |
| `IScopePathResolver` | `ScopedPermissionChecker` | A scope's current `ltree` path, for scoped records in other modules |
| `IScopeDirectory` | `ScopeDirectory` | The scopes the app-bar scope picker lists for the user |
| `IUserDirectory` | `UserDirectory` | Display names for user ids other modules store (audit stamps) |
| `IExternalIdentityResolver` | `ExternalIdentityResolver` | Keycloak sign-in (below) |
| `IDevUserSeeder` | `DevUserSeeder` | Gives the `DevUser` host's configured user the `administrator` role at the root |

Moving a scope publishes `ScopesMovedIntegrationEvent` through the outbox, so other modules' scoped records follow the new paths.

### Sign-in

**Keycloak authenticates; this module authorizes.** The realm grants no permissions. During the OpenID Connect callback the host calls `IExternalIdentityResolver`, which finds the `ua.users` row by the provider's `sub` — or links an existing row by username, or creates a new one with the seeded `viewer` role at the root — and answers with the Sergin user id. The host stamps only that id into the auth cookie, as the `sergin:user_id` claim. **No permission is stored in the cookie.**

Provisioning is an ordinary feature slice (`ProvisionExternalUserCommand`) through the same unit of work and pipeline as everything else. **It must never get a configuration requiring a permission**: it runs before sign-in completes, when the caller is still anonymous.

### Permissions

Each aggregate defines its permissions once, as `PermissionDefinition`s in `.Application.Configurations`:

| Definition | Name | Checked |
|---|---|---|
| `UserPermissions.Read` | `permission.ua.users.read` | On any scope the caller holds it |
| `UserPermissions.Manage` | `permission.ua.users.manage` | On any scope the caller holds it |
| `UserPermissions.AssignmentsManage` | `permission.ua.assignments.manage` | At the scope of the assignment |
| `ScopePermissions.Read` / `.Manage` | `permission.ua.scopes.read` / `.manage` | At the scope acted on |
| `RolePermissions.Read` / `.Manage` | `permission.ua.roles.read` / `.manage` | At the role's owner scope |

Deactivating a user, or reading their assignments, also requires that every live assignment of that user lies inside the caller's reach. A regional administrator cannot deactivate the root administrator. Putting a permission on a role is capped by what the caller holds in the role's owner scope.

### Aggregates

| Aggregate | Tables | Shape |
|---|---|---|
| `User` | `ua.users`, `ua.role_assignments` | `Create(UserName)` for a local user, `CreateFromExternalIdentity(...)` for a Keycloak one; `IsActive`/`Deactivate()`, an optional unique `ExternalId`, profile fields, and `RoleAssignment` child entities (`AssignRole`, `RevokeAssignment` — a revoked assignment keeps its row). |
| `Role` | `ua.roles`, `ua.role_permissions` | Owned by a scope; `Rename`, `SetInheritable`, `Grant`, `Revoke`. Permissions are an owned collection mapped onto SharedKernel's `Permission` value object. Audited, soft-deletable, versioned. |
| `Scope` | `ua.scopes` | A tree node with an `ltree` path; `Rename`, `MoveUnder` (re-paths the subtree in one save). The root is seeded and is the only one. Audited, soft-deletable, versioned. |

The migrations seed the root scope and two global roles, `administrator` and `viewer`.

### Pages and feature slices

| Page | What it does |
|---|---|
| `/ua/users`, `/ua/users/new`, `/ua/users/{id}` | User list, create, and detail with deactivate and the assignments panel (assign, revoke, effective permissions) |
| `/ua/scopes`, `/ua/scopes/new`, `/ua/scopes/{id}` | The scope tree the caller can see, create, and detail with rename, move, delete and the scope's roles |
| `/ua/roles/new`, `/ua/roles/{id}` | Create a role under a scope; detail with rename, inheritable, grant and revoke, delete |

Every slice lives under `<Aggregate>/Commands/<Feature>/` — queries included — in `.Application.Contracts` (the records) and `.Application` (the handler and its validator), mirrored in `.Infrastructure` and both presentation projects. Every write command has a FluentValidation validator. The minimal-API routes under `/ua/users`, `/ua/scopes` and `/ua/roles` are compiled and kept current but not served: the platform currently has no Web API host, and the Blazor pages dispatch to the same handlers in-process through `ISerginDispatcher`.

### Outbox

`UserAccessDbContext` implements `IOutboxDbContext` and calls `ApplyOutbox()`, and the `AddOutbox` migration creates `ua.outbox_messages` and `ua.inbox_messages`. `ScopesMovedTranslator` is the module's one translator today: it turns a scope move into `ScopesMovedIntegrationEvent`.

## This repository does not build on its own

There is deliberately no solution file, no `Directory.Build.props`, no `Directory.Packages.props` and no development configuration here. The module compiles only as a **git submodule** inside a host that also mounts [Sergin.SharedKernel](https://github.com/poursh/Sergin.SharedKernel) at the matching relative path. Today that host is [Sergin.MeterMinder](https://github.com/poursh/Sergin.MeterMinder), which mounts this repository at `src/Modules/UserAccess/` and SharedKernel at `src/SharedKernel/`.

The reason is correctness, not laziness: the project files reach SharedKernel through `..\..\..\SharedKernel\...`. Giving this repository its own nested SharedKernel for a "standalone" build would risk two copies of it compiling into one host — two `AggregateRoot`s, two `ISerginModule`s — and that failure mode is real.

To work on it:

```bash
git clone --recurse-submodules https://github.com/poursh/Sergin.MeterMinder.git
cd Sergin.MeterMinder
dotnet build Sergin.MeterMinder.slnx

# Edit under src/Modules/UserAccess/ as usual. A commit or push made inside that
# directory targets this repository, not MeterMinder; MeterMinder then bumps its pointer.
```

The host runs this module's integration tests (`tests/Sergin.MeterMinder.IntegrationTests.All`, for example `Authorization/ScopedRbacAcceptanceTests`) and its domain unit tests (`tests/Sergin.UserAccess.UnitTests`).

## Layout

| Project | Role |
|---|---|
| `Sergin.UserAccess.Domain` | `User`, `Role` and `Scope` aggregates, strongly-typed IDs, repository interfaces |
| `Sergin.UserAccess.Application.Contracts` | The command, query and response records — all a presentation layer needs |
| `Sergin.UserAccess.Application.Configurations` | Request policy (`ICommandConfiguration`), aggregate features, and the `<Aggregate>Permissions` definitions |
| `Sergin.UserAccess.Application` | Handlers, validators, query-repository interfaces, `IUserAccessUnitOfWork`, the authorization implementations (`ScopedPermissionChecker`, `ScopeDirectory`, the grant cache), `ExternalIdentityResolver`, `DevUserSeeder` |
| `Sergin.UserAccess.Infrastructure` | EF Core write repositories; raw-SQL query repositories, including the effective-grant read and `UserDirectory` |
| `Sergin.UserAccess.Infrastructure.Data` | `UserAccessDbContext`, entity configurations, value converters, migrations |
| `Sergin.UserAccess.Presentation.WebApi` | Minimal-API endpoints |
| `Sergin.UserAccess.Presentation.Blazor` | MudBlazor pages under `Users/`, `Scopes/` and `Roles/`, markup in `.razor`, code in `.razor.cs` |
| `Sergin.UserAccess` | `UserAccessModule` — the composition root implementing `ISerginWebApiModule` and `ISerginWebUiModule` |

Conventions, per-project detail and the reasoning behind each quirk are in [`.claude/CLAUDE.md`](.claude/CLAUDE.md). The design of scoped RBAC is in the host repository at `docs/superpowers/specs/2026-10-09-scoped-rbac-design.md`. `Users/` is the canonical reference slice for the whole platform — when in doubt about the right shape for a new feature elsewhere, read the matching file here first.

## License

[MIT](LICENSE) © Pejman Pourshirazi.
