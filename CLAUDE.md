# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A Unity UPM package (`com.gamedistrict.maxeventtracker`, Unity 2022.3+) that wraps a fixed set of predefined custom analytics events (ad activity, gaming, generic, social, subscription) and sends them through AppLovin MAX or Metica. The repo root *is* the package root — it is not a Unity project.

## Building / testing

There is no build script, test suite, or linter. Code only compiles inside a Unity project that consumes the package (via `Packages/manifest.json` with a `#vX.Y.Z` git tag, or as a local/embedded package). The `Samples~/` code additionally needs the AppLovin MAX SDK (and Metica for `MeticaSdkEventBackend`) in that host project. When changing code here, verify C# by reading carefully; you cannot run Unity in this environment.

Every new file/folder needs a matching Unity `.meta` file with a unique GUID (folders inside `Samples~/` and `Documentation~/` are exempt — Unity ignores `~` folders).

## Architecture

- **SDK-agnostic core.** `Runtime/` (assembly `GameDistrict.MaxEventTracker`, namespace `GameDistrict.EventTracking`) never references any SDK; its asmdef has no references. All sending goes through `IEventBackend` (`TrackEvent`, `IsRewardedAdReady`), which the game supplies to `MaxEventTracker.Initialize(backend)`. Keep it this way — projects install MAX either as UPM or under `Assets/`, so a hard reference would break one of them.
- **SDK adapters live in a sample**, `Samples~/SdkBackends` (`MaxSdkEventBackend`, `MeticaSdkEventBackend`, `EventBackendFactory`). Users import it into `Assets/` so it compiles next to their SDKs. MAX vs Metica is chosen at **runtime** via `TrackingEventsType` passed to `EventBackendFactory.Create`; Metica code is behind `#if METICA_SDK_PRESENT`, falling back to MAX with a warning.
- **`MaxEventTracker` is a static class** (no MonoBehaviour/prefab). State (`IsEnabled`, `Backend`, reported-failure set) is reset in a `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` method so it works with domain reload disabled. Event-group instances are get-only static properties created once; they are stateless and read `IsEnabled`/`Backend` from `MaxEventTracker`. `Initialize` also auto-fires the app-open event.
- **Event groups** (`Runtime/Events/*Events.cs`) derive from `EventGroup` (private-protected ctor; each subclass has an `internal` ctor). Each public `TrackXxx` method calls `EventGroup.Track(eventName, params)`, which: errors and no-ops if not initialized, logs `[MaxEventTracker] ...` to the console, and calls the backend inside a try/catch. **Tracking must never throw into game code** — backend exceptions go to `MaxEventTracker.ReportBackendFailure`, logged once per call key per session. Event names are snake_case strings (e.g. `level_start`); parameters are `Dictionary<string,string>` with keys like `value`, `resource_type`.
- **Editor** (`Editor/`, assembly `GameDistrict.MaxEventTracker.Editor`, namespace `GameDistrict.EventTracking.EditorTools`):
  - `MeticaSdkDetector` keeps the `METICA_SDK_PRESENT` scripting define in sync (Android/iOS/Standalone) by detecting the `Metica.SDK` asmdef or the `Assets/MeticaSdk` folder, re-syncing after every compilation and saving ProjectSettings immediately. Its `MeticaDefineBuildCheck` fails a build whose define is out of sync. Menu: `GameDistrict/MaxEventTracker/Metica SDK Detected`. It also strips the legacy `USE_METICA_TRACK_EVENTS` define.
  - `MaxEventTrackerLinkerProcessor` injects a `preserve="all"` link.xml (in `Library/GameDistrict.MaxEventTracker/`) — paired with `[assembly: AlwaysLinkAssembly]`/`[assembly: Preserve]` in `Runtime/AssemblyInfo.cs` so code survives any managed stripping level.

## Conventions

- Adding an event: add a `TrackXxx` method with an XML doc comment describing when to fire it to the appropriate `*Events.cs`, then document it in the README's method table + example for that group, and add a CHANGELOG entry under `[Unreleased]`.
- Version is duplicated in `package.json` (`version`), `MaxEventTracker.Version`, and README (`Version` line and the install `#vX.Y.Z` tag); keep them in sync when releasing.
- Public API changes are user-facing breaking changes for game projects — call them out as **Breaking** in `CHANGELOG.md`, as existing entries do.
