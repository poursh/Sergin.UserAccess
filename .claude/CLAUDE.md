# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Sergin.UserAccess is the UserAccess module (Postgres schema `ua`) for the **Sergin** platform — a .NET 10 modular monolith whose **MeterMinder** module is a Head-End System (HES) for smart electricity/gas/water meters. This module owns user identity/access; currently the `Users` aggregate.

**This repo is embed-only.** There is no solution file, no `Directory.Build.props`/`Directory.Packages.props`, no dev config here — see `README.md` for why. It only compiles as a git submodule inside a host repo (today: [Sergin.MeterMinder](https://github.com/poursh/Sergin.MeterMinder)) that also provides a `Sergin.SharedKernel` submodule at the matching relative path. When working in this repo, assume you're editing it from inside that host checkout — `dotnet build`/`dotnet test` commands run against the *host's* solution file, not one in this repo.

## Per-project layering

- **`Sergin.UserAccess.Domain`** — aggregates/entities, strongly-typed IDs, repository interfaces. Depends only on `Sergin.SharedKernel.Domain`. Aggregates are built via a private/parameterless constructor + a `static Create(...)` factory method (e.g. `User.Create(UserName)`) — no public setters; mutate via named methods on the aggregate (e.g. `User.Deactivate()`).
  - ID generation always uses `Guid.CreateVersion7()`, never `Guid.NewGuid()` — e.g. `new UserInternalId(Guid.CreateVersion7())`.
  - `Create(...)` returns via **object-initializer syntax** against the private parameterless constructor (`new User { Id = ..., UserName = userName, IsActive = true }`), not a parameterized constructor call.
  - Strongly-typed IDs/value objects are declared as trailing `sealed record`s in the **same file** as their owning aggregate (e.g. `UserInternalId` and `UserName` both live in `User.cs`), not split into separate files.
- **`Sergin.UserAccess.Application.Contracts`** — the module's MediatR command/query request and response records only (e.g. `CreateUserCommand`, `CreateUserCommandResponse`, `GetUserByIdQueryCommand`, `UserQueryResponse`, `GetUserListItem`, `DeactivateUserCommand`, `DeactivateUserCommandResponse`), moved verbatim out of `.Application` — same namespace, same domain-typed constructor arguments, same `[RequiredPermissions]` attributes. References only `SharedKernel.Application` and this module's own `.Domain`. Exists so presentation layers (`.Presentation.WebApi`, `.Presentation.Blazor`) can depend on request/response shapes without pulling in handlers, repository interfaces, or `IUserAccessUnitOfWork` — those all stay in `.Application`. Carries `UserAccessApplicationContractsAssemblyReference`, exposing `typeof(...).Assembly` for `ISerginModule.ContractsAssembly`. See the [Sergin.MeterMinder](https://github.com/poursh/Sergin.MeterMinder) repo's `docs/superpowers/specs/2026-08-22-application-contracts-split-design.md` for the full design.
- **`Sergin.UserAccess.Application`** — MediatR handlers, query repository interfaces, `IUserAccessUnitOfWork`. References `.Application.Contracts` (added by the same split) for the request/response types its handlers implement `ICommandHandler<TCommand, TResponse>`/`IQueryHandler<TQuery, TResponse>` against. Feature folders hold the full slice under `<Aggregate>/Commands/<Feature>/...` — **queries live under `Commands/` too**, not a separate `Queries/` folder.
- **`Sergin.UserAccess.Infrastructure`** — write-side repositories (EF Core) and read-side query repositories (raw SQL via `IDbConnectionFactory`).
  - Generic PK lookup uses the array-args overload: `dbContext.Set<T>().FindAsync([id, cancellationToken], cancellationToken: cancellationToken)`.
  - Aggregate-specific lookups (`GetByUserName`) use `SingleOrDefaultAsync(x => x.Field == value, cancellationToken)` and are added directly to the repository interface (`IUserRepository`).
- **`Sergin.UserAccess.Infrastructure.Data`** — the module's `DbContext` (`UserAccessDbContext`, schema `ua`), `IEntityTypeConfiguration`s, value converters, migrations.
- **`Sergin.UserAccess.Presentation.WebApi`** — minimal-API endpoints implementing `IEndpoint`.
- **`Sergin.UserAccess.Presentation.Blazor`** — Razor components for the Blazor Server UI (`Microsoft.NET.Sdk.Razor`). Feature-organized by aggregate under `<Aggregate>/Pages/`, with markup and code split into `.razor` and `.razor.cs` files. Each component exposes its navigation item through the module's `<Aggregate>Navigation` class, implementing `ISerginUiNavigation` to surface it on startup; the host reflects over modules' `UiAssembly` to collect them. Pages are also registered in the module's `NavItems` for routing. See **Blazor UI** section below for per-page conventions.
- **`Sergin.UserAccess`** (no-suffix composition root) — implements `ISerginWebApiModule` and `ISerginWebUiModule` from `Sergin.SharedKernel.Modules` (`UserAccessModule` class): `Schema`, `ApplicationAssembly`, `ContractsAssembly` (points to `Sergin.UserAccess.Application.Contracts`), `UiAssembly` (points to `Sergin.UserAccess.Presentation.Blazor`), `AddServices` (calls `AddModuleDbContext<TContext, TIContext, TIUnitOfWork>` plus per-aggregate `Add<X>Dependencies()`), `MigrateAsync`, `MapEndpoints` (per-aggregate `Map<X>Endpoints()`), and `NavItems` (list of UI navigation items collected from the Blazor project).

## CQRS split

- **Writes**: endpoint → MediatR `ICommand` → `ICommandHandler` → domain `AggregateRoot` factory/behavior method → `IRepository` (EF Core) → `IUnitOfWork.SaveChangesAsync`.
- **Reads**: query handlers use dedicated query-repository interfaces (`I<Feature>QueryRepository`) backed by **raw SQL through `IDbConnectionFactory`** (Dapper-style `QuerySingleOrDefaultAsync`/`QueryMultipleAsync`), bypassing EF entirely. A query handler maps a `null` result to `Error.NotFound()`.
  - Each query method opens its own `using DbConnection connection = await connectionFactory.CreateConnectionAsync();`.
  - SQL is a raw `"""..."""` string literal; snake_case columns are aliased to match the response record's exact property casing (`SELECT user_name AS userName FROM ua.users WHERE id = @Id;`).
  - List queries batch **two** statements through one `QueryMultipleAsync` call — `SELECT count(*) ...;` then the paged `SELECT ... LIMIT @PageSize OFFSET @Offset;` — read off the same `GridReader` (`ReadSingleAsync<int>()` then `ReadAsync<TItem>()`), wrapped as `new ListQueryResponse<TItem>(list, count)`.
- **List-query features have no dedicated request record.** `GetUserListQueryCommandHandler` implements `IListQueryHandler<TItem>` directly against the shared generic `ListQuery<TItem>` — there is no `GetUserListQueryCommand` type to attribute `[RequiredPermissions]` on. This is a structural gap in the shared `ListQuery<T>` type (defined in `Sergin.SharedKernel.Application`), not an inconsistently-applied convention.
- **`.Produces<TResponse>()` is called on Create/GetList endpoints but omitted on GetOne endpoints.**
- **Endpoint route strings never include the schema segment** (`/users`, not `/ua/users`) — the host adds the schema prefix once via `MapGroup(module.Schema)`.

## Cross-cutting conventions

- **Results**: handlers return `ErrorOr<T>` (global-imported). Endpoints call `.ToApiResult()`.
- **Permissions**: `[RequiredPermissions("permission.ua.<resource>.<action>")]` on a command/query record — opt-in per slice, most commands have none.
- **Validation**: FluentValidation is wired but optional — add an `AbstractValidator<T>` alongside a command/query only when the feature needs input validation beyond what the domain factory already guards.
- **Domain events**: `AggregateRoot` supports `Raise(IDomainEvent)`, but **no aggregate in this module calls it yet** — present-but-unused infrastructure.
- **Naming/sealing**: response records are `<Feature>CommandResponse` for commands, `UserQueryResponse` for a single-item query; list items are `GetUserListItem`. GetOne query/request records keep the blended `Get<Aggregate>ByIdQueryCommand` suffix even though they implement `IQuery<T>`. Application-layer commands/queries/responses are always `sealed record`; Presentation-layer `[FromBody]` request DTOs are plain `record`, not sealed. Handler classes are `internal sealed class`; **endpoint classes are `internal class`, never sealed**.
- **Strongly-typed IDs**: `record` wrappers (e.g. `UserInternalId(Guid)`) mapped to columns via EF value converters (see `Sergin.SharedKernel`'s `.claude/CLAUDE.md` for the converter template).
- **Database schema**: `ua`, set via `HasDefaultSchema` in `UserAccessDbContext` + a per-schema migrations history table. `UseSnakeCaseNamingConvention()` maps PascalCase members to snake_case columns.
- **User context**: `InternalUserContextFactory` (in SharedKernel) currently returns a `SYSTEM`/`ANONYMOUS` stub — real auth isn't wired yet.
- **Local variable typing**: declare a local as the narrowest interface its actual usage needs — e.g. `IReadOnlyCollection<T>` over `List<T>`. `UserQueryRepository.GetListAsync` materializes Dapper's `IEnumerable<T>` result as `IReadOnlyCollection<TItem> list = [.. await res.ReadAsync<TItem>()];`.
- Check each project's `GlobalUsings.cs` before adding `using` statements — many namespaces (`ErrorOr`, `Sergin.SharedKernel.*`) are already global.

## Blazor UI

Pages in `Sergin.UserAccess.Presentation.Blazor` follow these rules:

- **Route prefix is `/ua/`** — Razor `@page` directives are compile-time constants, so unlike minimal-API endpoints (which the host prefixes once via `MapGroup(schema)`), the schema prefix must be written into each page directive (e.g., `@page "/ua/users"`, `@page "/ua/users/new"`). A host-level startup route guard reflects over every module's `UiAssembly` and throws, naming any violating component. **Always match the module's schema in every `@page` string.**
- **`.razor` files contain markup only; all C# logic lives in `.razor.cs` code-behind.** Code inside `@code {}` blocks becomes part of generated output and escapes analyzer inspection, creating an unaudited hole in a repo where warnings-as-errors is enforced. Keep that hole empty.
- **Blazor pages inject `ISerginDispatcher` (`Sergin.SharedKernel.Presentation.Blazor.Dispatching`), never MediatR's `ISender`/`IMediator` directly.** In Blazor Server, "scoped" means the SignalR circuit's lifetime (hours), not a single HTTP request. Resolving MediatR's `ISender` off the circuit scope would share one `DbContext` across every user interaction, creating an unbounded change tracker, stale reads, and "a second operation was started on this context" errors on concurrent renders. `ISerginDispatcher.SendAsync(...)` opens a fresh DI scope per send, isolating each operation's `DbContext`. Inject it via `[Inject] ISerginDispatcher Dispatcher { get; set; }` in the code-behind (see `CreateUserPage.razor.cs`). **WebApi endpoints inject `ISender` directly** (see `CreateUserEndpoint`) — a minimal-API request already gets its own request-scoped DI container per call, so it has the same isolation for free without needing the dispatcher's fresh-scope trick.
- **List pages use `MudTable` with `ServerData` and paging only** — no sort, filter, or search controls. Not because the shared type lacks the fields: `ListQuery<T>` *does* carry `Term`, `Filtering` and `Sorting`. The plumbing below it drops them — `ListQueryRequestModel.ToListQuery<T>()` forwards `Term` but not `Filtering`/`Sorting`, `SendListAsync` forwards none of the three, and `UserQueryRepository.GetListAsync` binds only `PageSize`/`Offset` with a hardcoded `ORDER BY id`. So a control bound to any of them would silently do nothing, which is worse than omitting it; wiring them through is a read-side feature, not a UI change. Note that `MudTable`'s `TableState.Page` is 0-based while Sergin's `ListQuery.PageIndex` is 1-based — adjust when building the query.
- **`UserQueryResponse` carries `(Guid Id, string UserName)` only** — it has no `IsActive` field, even though `User.IsActive` exists in the domain and the `Deactivate()` command mutates it. The detail page cannot display the user's active state, and the deactivate action gives no pre-deactivation confirmation or post-action feedback beyond a snackbar notification. Widening the response record to include `IsActive` is a real API contract change belonging to its own query feature slice; it is not a simple omission to fix incidentally.

## `Users` aggregate

`Sergin.UserAccess.Domain/Users/User.cs` — `AggregateRoot<UserInternalId>`, private ctor + `static Create(UserName)` factory. `IsActive` defaults to `true` on creation and is flipped by the `Deactivate()` method.

Implemented feature slices (`Users/Commands/<Feature>/` in Application, mirrored in Infrastructure/Presentation):

| Feature | Kind | Route | Permission |
|---|---|---|---|
| `Create` | command | `POST /ua/users` | none |
| `DeactivateUser` | command | `POST /ua/users/{userId:guid}/deactivate` | none |
| `GetOne` | query | `GET /ua/users/{userId:guid}` | `permission.ua.users.read` |
| `GetList` | query | `GET /ua/users` (`[AsParameters] ListQueryRequestModel`) | none |

Only `GetOne` carries a `[RequiredPermissions]` attribute — that's the current state of the module, not a rule that only reads need it. Add the attribute to new slices that should be protected; don't remove it from `GetOne` to "match" the others.

### Repositories

- `IUserRepository` (`Domain/Users/`) extends the generic `IRepository<User, UserInternalId>` with one extra method, `GetByUserName(UserName)` — the precedent for adding aggregate-specific lookups to the repository interface when generic CRUD isn't enough, implemented by `UserRepository` (EF Core) in Infrastructure.
- Query repositories are split one-interface-per-feature (`IGetUserQueryRepository`, `IGetUserListQueryRepository`, plus a module-wide `IUserAllQueryRepository`), all implemented by a single `UserQueryRepository` class using `IDbConnectionFactory` + raw SQL. Follow this split (new interface per query feature, one class implementing all of them) rather than one fat repository interface.
