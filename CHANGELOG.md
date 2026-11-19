# Changelog

## [Unreleased]
- `METICA_SDK_PRESENT` changes are written to ProjectSettings.asset immediately, so commits and CI builds see them
  without a manual project save. New `MeticaDefineBuildCheck` (preprocess build): if the define doesn't match whether
  Metica is in the project for an Android/iOS/Standalone build, it fixes and saves the define and fails the build
  with an explanation (scripts are already compiled with the old defines, so this build can't be saved).
- Tracking can no longer break game code: exceptions from the SDK backend (`TrackEvent`, `IsRewardedAdReady`) are
  caught. Each failing call is logged once per session with its stack trace. If the readiness check throws,
  `TrackRewardedAdOpportunity` treats the ad as not ready and skips the event.
- Event groups are now created once (get-only static properties) instead of by both the static constructor and
  the `SubsystemRegistration` reset. The reset now only clears `IsEnabled` and `Backend`.
- Metica detection status is now shown instead of a "Refresh Metica Detection" command: the menu item
  `GameDistrict/MaxEventTracker/Metica SDK Detected` is checked when Metica is detected, and clicking it shows a
  detected / not-detected dialog with the source (assembly or `Assets/MeticaSdk` folder). There's also one console
  status line per Editor session.
- **Breaking, renamed** to avoid the word "Analytics", which other SDKs in the project also use, and to avoid
  AppLovin's own global `MaxEvents` constants class:
  - package `com.gamedistrict.maxanalytics` → `com.gamedistrict.maxeventtracker` (display name MaxEventTracker)
  - class `MaxAnalytics` → `MaxEventTracker`; namespace `Monetization.Runtime.Analytics` → `GameDistrict.EventTracking`
    (editor: `GameDistrict.EventTracking.EditorTools`)
  - asmdefs `GameDistrict.MaxAnalytics(.Editor)` → `GameDistrict.MaxEventTracker(.Editor)`
  - `IAnalyticsBackend` → `IEventBackend`; `MaxSdkAnalyticsBackend` / `MeticaSdkAnalyticsBackend` /
    `SdkAnalyticsBackendFactory` → `MaxSdkEventBackend` / `MeticaSdkEventBackend` / `EventBackendFactory`, now in
    `GameDistrict.EventTracking` (they used to be in the global namespace)
  - log prefix `[MaxEventTracker]`, menu `GameDistrict/MaxEventTracker/…`, linker file
    `Library/GameDistrict.MaxEventTracker/link.xml`
- **Breaking:** `MaxAnalytics` is now a static class: no MonoBehaviour, no singleton, no prefab. Use
  `MaxAnalytics.Gaming...` instead of `MaxAnalytics.Instance.Gaming...`. State (IsEnabled, Backend, event groups)
  is reset at `RuntimeInitializeLoadType.SubsystemRegistration`. Deleted `Runtime/MaxAnalytics.prefab`; remove the
  prefab instance from your bootstrap scene. Event group constructors are internal.
- **Breaking:** the package no longer references any SDK. `MaxAnalytics.Initialize()` is now
  `Initialize(IAnalyticsBackend backend)`; the game supplies an adapter that sends events through its own
  MAX/Metica install, so it works whether MAX is a UPM package or a folder under Assets/.
- New `IAnalyticsBackend` interface (`TrackEvent`, `IsRewardedAdReady`) and `MaxAnalytics.Backend`.
- New sample **SDK Backends** (`Samples~/SdkBackends`): `MaxSdkAnalyticsBackend`, `MeticaSdkAnalyticsBackend`
  (compiled only with `METICA_SDK_PRESENT`) and `SdkAnalyticsBackendFactory`. Asking for Metica when it isn't
  imported logs a warning and falls back to the MAX backend.
- Metica vs AppLovin is now chosen at **runtime**, so remote config can switch it in a release build. Replaces the
  build-time `USE_METICA_TRACK_EVENTS` define. New `TrackingEventsType` enum (`Max`, `Metica`). The game passes the
  type of the SDK it initialized to `SdkAnalyticsBackendFactory.Create`, and gives the result to `Initialize`.
- Whether Metica is imported is detected automatically: new editor script `MeticaSdkDetector` keeps a
  `METICA_SDK_PRESENT` define in sync with the presence of the `Metica.SDK` assembly, falling back to the
  `Assets/MeticaSdk` folder (Android/iOS/Standalone).
  It also removes a leftover `USE_METICA_TRACK_EVENTS`.
- Removed the manual `GameDistrict/MaxAnalytics/Use Metica | Use Applovin` menu (`AnalyticsSdkMenu`). Added
  `GameDistrict/MaxAnalytics/Refresh Metica Detection`.
- Removed the `MaxSdk.Scripts` asmdef reference and the `com.applovin.mediation.ads` package dependency.
- Package code is never stripped from player builds, at any Managed Stripping Level (Mono or IL2CPP):
  `[assembly: AlwaysLinkAssembly]` + `[assembly: Preserve]` on the runtime assembly, plus an
  `IUnityLinkerProcessor` that injects a `preserve="all"` link.xml for `GameDistrict.MaxAnalytics`.

## [0.2.0]
- Converted from `Assets/MaxAnalytics` to a UPM package (`com.gamedistrict.maxanalytics`).
- Runtime code compiles into `GameDistrict.MaxAnalytics` (references `MaxSdk.Scripts`); editor menu into `GameDistrict.MaxAnalytics.Editor`.
