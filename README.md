# Sergin.UserAccess

The UserAccess module (schema `ua`) for the [Sergin](https://github.com/poursh/Sergin.MeterMinder) platform, whose **MeterMinder** module is a Head-End System (HES) for smart electricity/gas/water meters. Owns user identity and access: the `Users` and `Roles` aggregates, and the permission set the rest of the platform authorizes against.

It is also the module that turns an external sign-in into a Sergin user. It implements SharedKernel's
`IExternalIdentityResolver`, which the host calls during the OpenID Connect callback: an unseen provider
`sub` gets a `ua.users` row and the seeded `viewer` role, and the resolver hands back that user's permissions
for the host to stamp into the auth cookie as claims. Keycloak authenticates; **this module authorizes**.
The migration seeds the `administrator` and `viewer` roles; there is no role-administration UI yet, so
changing who holds which role means editing `ua.user_roles` directly.

Feature slices today: `Users/Commands/{Create, GetOne, GetList, DeactivateUser, ProvisionExternalUser}`,
plus the Blazor pages for them under `Sergin.UserAccess.Presentation.Blazor`.

## This repo is embed-only — it does not build standalone

There is deliberately **no solution file, no `Directory.Build.props`/`Directory.Packages.props`, and no dev config** in this repo. It only compiles as a **git submodule** embedded inside a host repo that also provides a `Sergin.SharedKernel` submodule at a matching relative path — today, that host is [Sergin.MeterMinder](https://github.com/poursh/Sergin.MeterMinder), which mounts this repo at `src/Modules/UserAccess/` and SharedKernel at `src/SharedKernel/`.

Why: this module's project files reach SharedKernel via a relative `ProjectReference` path (`..\..\..\SharedKernel\...`) that assumes that exact folder depth. Giving this repo its own nested copy of SharedKernel for "standalone" building would risk two different SharedKernel copies getting compiled into the same host app — a real correctness risk, not a hypothetical one. See `Sergin.MeterMinder`'s root `CLAUDE.md` for the full reasoning.

To work on this code:
```bash
git clone --recurse-submodules https://github.com/poursh/Sergin.MeterMinder.git
# edit files under src/Modules/UserAccess/ (this repo) as normal;
# commit/push from inside that submodule folder targets this repo, not MeterMinder.
```

See `.claude/CLAUDE.md` for this module's architecture and conventions.

## License

[MIT](LICENSE) © Pejman Pourshirazi.
