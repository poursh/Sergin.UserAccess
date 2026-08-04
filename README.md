# Sergin.UserAccess

The UserAccess module (schema `ua`) for the [Sergin](https://github.com/poursh/Sergin.MeterMinder) platform, whose **MeterMinder** module is a Head-End System (HES) for smart electricity/gas/water meters. Owns user identity/access — currently the `Users` aggregate.

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
