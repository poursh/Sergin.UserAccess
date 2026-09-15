# Sergin.UserAccess

**The identity and access module of the Sergin platform — schema `ua`. It owns users, roles and the permission set every other module authorizes against, and it is the half of Keycloak sign-in that decides what a signed-in person may do.**

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/)
[![Schema](https://img.shields.io/badge/schema-ua-336791)](Sergin.UserAccess.Infrastructure.Data)
[![Embed-only](https://img.shields.io/badge/builds-embed--only-B45309)](#this-repository-does-not-build-on-its-own)
[![License: MIT](https://img.shields.io/badge/License-MIT-2E7D32)](LICENSE)

Sergin is a .NET 10 modular monolith whose **MeterMinder** module is a head-end system for smart electricity, gas and water meters. This module is the one every other module depends on for authorization: Keycloak proves who someone is, and `ua` answers what they are allowed to do.

## What it does

**Keycloak authenticates; this module authorizes.** The realm grants no permissions. During the OpenID Connect callback the host calls this module's `IExternalIdentityResolver`, which:

1. finds the `ua.users` row by the provider's `sub`, or creates one and assigns it the seeded `viewer` role;
2. reads that user's permissions out of `ua.role_permissions`;
3. hands them back for the host to stamp into the auth cookie as `sergin:permission` claims.

A permission check afterwards reads claims only — no database work — so a change to someone's roles takes effect at their next sign-in. Provisioning runs as an ordinary feature slice (`ProvisionExternalUserCommand`) through the same unit of work and pipeline as everything else, with one rule: **it must never carry `[RequiredPermissions]`**, because it runs before sign-in completes, when the caller is still anonymous.

### Aggregates

| Aggregate | Table | Shape |
|---|---|---|
| `User` | `ua.users` | `Create(UserName)` for a local user, `CreateFromExternalIdentity(...)` for a Keycloak one; `IsActive`, `Deactivate()`, an optional `ExternalId` (the provider's `sub`), profile fields, and an owned `Roles` collection (`ua.user_roles`). |
| `Role` | `ua.roles` | `Create(RoleName, IEnumerable<Permission>)` with `Grant`/`Revoke`; permissions are an owned collection (`ua.role_permissions`) mapped straight onto SharedKernel's `Permission` value object. |

The `AddRolesAndExternalIdentity` migration seeds two roles, `administrator` and `viewer`. **There is no role-administration UI yet** — no role feature slices, endpoints or pages — so changing who holds which role means editing `ua.user_roles` directly. `Role.DefaultRoleName` (`viewer`) is a constant on purpose; it becomes configurable when role administration lands.

### Feature slices

Everything lives under `Users/Commands/<Feature>/` — queries included — in `.Application.Contracts` (the request and response records) and `.Application` (the handler), mirrored in `.Infrastructure` and both presentation projects.

| Feature | Kind | HTTP route (unhosted) | Blazor page | Permission |
|---|---|---|---|---|
| `Create` | command | `POST /ua/users` | `/ua/users/new` | — |
| `GetOne` | query | `GET /ua/users/{userId:guid}` | `/ua/users/{Id:guid}` | `permission.ua.users.read` |
| `GetList` | query | `GET /ua/users` | `/ua/users` | `permission.ua.users.read` |
| `DeactivateUser` | command | `POST /ua/users/{userId:guid}/deactivate` | from the detail page | — |
| `ProvisionExternalUser` | command | — dispatched in-process from the OIDC callback | — | none, by design |

The HTTP routes are compiled and kept current but not served: the platform currently has no Web API host, and the Blazor pages dispatch to the same handlers in-process through `ISerginDispatcher`.

### Outbox

`UserAccessDbContext` implements `IOutboxDbContext` and calls `ApplyOutbox()`, and the `AddOutbox` migration creates `ua.outbox_messages` and `ua.inbox_messages`. That opt-in is what registers this module's inbox and relay source with the host's `OutboxRelayService`. **No integration event, translator or handler is declared here yet** — the tables are ready for the first one.

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

## Layout

| Project | Role |
|---|---|
| `Sergin.UserAccess.Domain` | `User` and `Role` aggregates, strongly-typed IDs, `IUserRepository` |
| `Sergin.UserAccess.Application.Contracts` | The command, query and response records — all a presentation layer needs |
| `Sergin.UserAccess.Application` | Handlers, query-repository interfaces, `IUserAccessUnitOfWork`, `ExternalIdentityResolver` |
| `Sergin.UserAccess.Infrastructure` | EF Core `UserRepository`; `UserQueryRepository` over raw SQL |
| `Sergin.UserAccess.Infrastructure.Data` | `UserAccessDbContext`, entity configurations, value converters, migrations |
| `Sergin.UserAccess.Presentation.WebApi` | Minimal-API endpoints |
| `Sergin.UserAccess.Presentation.Blazor` | MudBlazor pages under `Users/Pages/`, markup in `.razor`, code in `.razor.cs` |
| `Sergin.UserAccess` | `UserAccessModule` — the composition root implementing `ISerginWebApiModule` and `ISerginWebUiModule` |

Conventions, per-project detail and the reasoning behind each quirk are in [`.claude/CLAUDE.md`](.claude/CLAUDE.md). This module is the canonical reference slice for the whole platform — when in doubt about the right shape for a new feature elsewhere, read the matching file here first.

## License

[MIT](LICENSE) © Pejman Pourshirazi.
