# qDesk

A desktop brokerage platform built with .NET and WPF: client management, trading accounts, market
simulation, order execution, risk management, real-time market data and a TradingView-inspired chart.

Work in progress. Right now the app starts, composes itself through the generic host and shows a
shell window. See [Status](#status).

## Requirements

- Windows 10 or 11 (the WPF app targets `net10.0-windows`)
- .NET 10 SDK, version 10.0.100 or a newer feature band

The SDK version is pinned in `global.json` with `rollForward: latestFeature`, so a newer 10.0.x SDK
works without changes.

```powershell
dotnet --info
```

## Getting started

```powershell
git clone <repo-url>
cd qdesk
dotnet restore
dotnet build
```

## Running

```powershell
dotnet run --project src/qDesk.Desktop
```

The launch profile sets `DOTNET_ENVIRONMENT=Development`, so the title bar reads
`qDesk — Trading Desk (Development)`. That suffix is a quick way to confirm the Development
configuration layer was picked up.

To run a build directly:

```powershell
src\qDesk.Desktop\bin\Debug\net10.0-windows\qDesk.Desktop.exe
```

Launched that way there is no `DOTNET_ENVIRONMENT`, so the app runs as `Production`.

## Configuration

Settings are layered. Later sources override earlier ones.

| Source | Committed | Use |
| --- | --- | --- |
| `appsettings.json` | yes | Defaults |
| `appsettings.Development.json` | yes | Development overrides |
| `appsettings.Local.json` | no, git-ignored | Your machine only |
| Environment variables | n/a | CI and one-off overrides |

Put connection strings, credentials and anything machine-specific in `appsettings.Local.json`. It is
git-ignored so it cannot be committed by accident.

Configuration files are read from the directory holding the executable, not the working directory.

## Tests

```powershell
dotnet test --solution qDesk.slnx
```

A single project:

```powershell
dotnet test --project tests/qDesk.Architecture.Tests/qDesk.Architecture.Tests.csproj
```

The `--solution` and `--project` flags are required by the .NET 10 test runner
(Microsoft.Testing.Platform). Plain `dotnet test qDesk.slnx` will tell you so.

## Code style

Style and analyzer rules live in `.editorconfig` and are enforced during the build. Warnings are
errors, so a formatting violation fails the build rather than lingering in a review.

```powershell
dotnet format qDesk.slnx                       # fix
dotnet format qDesk.slnx --verify-no-changes   # check, same as CI
```

Line endings are LF on every platform, including Windows. `.gitattributes` controls what lands in
your working tree and `.editorconfig` has to agree with it.

## Project layout

| Project | Target | Depends on |
| --- | --- | --- |
| `src/qDesk.Domain` | `net10.0` | nothing |
| `src/qDesk.Application` | `net10.0` | Domain |
| `src/qDesk.Infrastructure` | `net10.0` | Application |
| `src/qDesk.Desktop` | `net10.0-windows` | Application, Infrastructure |

Only the desktop project is Windows-specific. `tests/qDesk.Architecture.Tests` fails the build if
that table stops being true, so the layering is enforced rather than documented.

CI builds the platform-agnostic projects on Linux and the full solution on Windows.

## Troubleshooting

**`FileLoadException: ... An Application Control policy has blocked this file. (0x800711C7)`**

Windows 11 Smart App Control blocks unsigned binaries without an established reputation, which
includes assemblies you compiled a moment ago. It breaks both `dotnet run` and `dotnet test`. Turn it
off under Windows Security > App & browser control > Smart App Control settings. You cannot turn it
back on afterwards without resetting Windows.

**`IDE0055` or `ENDOFLINE` errors in CI but not locally**

Line endings. Check with `git ls-files --eol`; every text file should report `i/lf w/lf`.
The build and `dotnet format` catch different subsets of this, which is why CI runs both.

**Style errors after editing in a tool that writes CRLF**

Run `dotnet format qDesk.slnx`.

## Status

Done:

- Four-layer solution with boundaries enforced by architecture tests
- Central package management, pinned SDK, analyzers as errors
- CI on Windows and Linux, including a formatting gate
- Generic host: DI, layered configuration, logging, application lifetime

Next:

- PostgreSQL via Docker Compose, EF Core and the first migration
- First vertical slice: client, trading account, deposit
- MVVM navigation and a read model the grid can bind to
