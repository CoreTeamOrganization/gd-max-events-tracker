# MaxEventTracker

A lightweight wrapper for tracking predefined user events through the AppLovin MAX SDK (or Metica). Events are grouped by category and exposed through a singleton.

- **Version:** 0.2
- **Package:** `com.gamedistrict.maxeventtracker`
- **Namespace:** `GameDistrict.EventTracking` (the package and the SDK Backends sample). Add `using GameDistrict.EventTracking;`.
- **Dependency:** none at package level. The package never references an SDK directly — the game passes an `IEventBackend` adapter (see *Connect your SDK* below), so it works however MAX is installed (UPM package or a folder under `Assets/`).


## Installation

Install through Unity's **Package Manager**:

1. Open **Window → Package Manager**.
2. Click **+** (top-left) → **Add package from git URL…**
3. Paste the URL, pinned to a release tag, and click **Add**:

   ```
   https://github.com/CoreTeamOrganization/gd-max-events-tracker.git#v0.2.0
   ```

4. Select **MaxEventTracker** in the list → **Samples** → **SDK Backends** → **Import**. This copies the MAX/Metica
   backends and `EventBackendFactory` into your `Assets/` once.

To upgrade, add the package again with the new tag. Delete the old `Assets/Samples/MaxEventTracker/<old version>/`
folder before re-importing the sample, or the classes will be defined twice.

---

## Setup

> **No scene setup.** `MaxEventTracker` is a plain static class: there's no prefab or GameObject to place. Its state is reset at `RuntimeInitializeLoadType.SubsystemRegistration` (before any scene loads) on every play session, so it starts disabled and clean even in the Editor with domain reload turned off. Call the static members directly, e.g. `MaxEventTracker.Gaming.TrackLevelStart(3)`.

### Connect your SDK (`IEventBackend`)

MaxEventTracker sends every event through an `IEventBackend` that **your game** provides:

```csharp
public interface IEventBackend
{
    void TrackEvent(string eventName, IDictionary<string, string> parameters);
    bool IsRewardedAdReady(string adUnitId);
}
```

Ready-made adapters ship as a package sample: **Package Manager → MaxEventTracker → Samples → SDK Backends → Import**. It copies three files into `Assets/Samples/...`, where they compile alongside your SDKs:

| File | Purpose |
| --- | --- |
| `MaxSdkEventBackend.cs` | AppLovin MAX: `MaxSdk.TrackEvent`, `MaxSdk.IsRewardedAdReady`. |
| `MeticaSdkEventBackend.cs` | Metica: `MeticaSdk.Ads.Max.TrackEvent`, `MeticaSdk.Ads.IsRewardedReady`. Compiled only while Metica is imported. |
| `EventBackendFactory.cs` | `Create(TrackingEventsType)` returns the backend for `TrackingEventsType.Max` or `TrackingEventsType.Metica`. |

You can move them anywhere in your game's scripts, or write your own adapter for another SDK.

### AppLovin MAX or Metica

A game initializes exactly one of Metica or AppLovin per session (for example, chosen by Firebase remote config), so one build can ship both. Whichever SDK initializes creates its own backend through the factory:

```csharp
// AppLovin MAX initialized
MaxEventTracker.Initialize(EventBackendFactory.Create(TrackingEventsType.Max));

// Metica initialized
MaxEventTracker.Initialize(EventBackendFactory.Create(TrackingEventsType.Metica));
```

**Is Metica imported?** You don't need to do anything. While the Metica SDK is in the project, the package's editor script adds the `METICA_SDK_PRESENT` scripting define (Android, iOS, Standalone), and removes it when Metica is deleted. It looks for Metica's `Metica.SDK` assembly first. If that isn't found, it checks for Metica's import folder, `Assets/MeticaSdk` (Metica ships only as a `.unitypackage`; there's no official UPM release yet). It re-checks on Editor load and after every script compile, including failed ones. Without Metica, `MeticaSdkEventBackend` isn't compiled, and `Create(TrackingEventsType.Metica)` logs a warning and returns the AppLovin MAX backend instead.

**Is Metica detected right now?** There are two indicators:
- **Menu:** `GameDistrict → MaxEventTracker → Metica SDK Detected` has a ✓ checkmark when Metica is detected and none when it isn't. Clicking it re-checks and opens a dialog saying how Metica was found (assembly or folder) and what that means for `EventBackendFactory`.
- **Console:** once per Editor session, a line saying `[MaxEventTracker] Metica SDK detected (…)` or `… not detected …`, plus a line whenever the define is added or removed.

### Initialize after your ad SDK

`MaxEventTracker` must be initialized **after the ad SDK (MAX or Metica) finishes initializing**. Event tracking is gated behind `Initialize(backend)` and no-ops (with an error log) until it runs. `Initialize` also tracks the app-open event automatically.

**AppLovin MAX** — call it from the MAX initialization callback:

```csharp
void Start()
{
    MaxSdkCallbacks.OnSdkInitializedEvent += OnMaxInitialized;

    MaxSdk.SetSdkKey("YOUR_SDK_KEY");
    MaxSdk.InitializeSdk();
}

private void OnMaxInitialized(MaxSdkBase.SdkConfiguration config)
{
    // MAX is ready — initialize event tracking here.
    MaxEventTracker.Initialize(EventBackendFactory.Create(TrackingEventsType.Max));
}
```

**Metica** — call it once Metica's ads initialization succeeds:

```csharp
protected override async Task OnInitializeAsync(bool testAds)
{
    MeticaAds.RevenueCallbackDelivery = CallbackDelivery.NativeThread;
    var mediationInfo = new MeticaMediationInfo(MeticaMediationInfo.MeticaMediationType.MAX, AppKey);
    var result = await MeticaInitializer.InitializeAds(mediationInfo);

    // Success — Metica is ready, initialize event tracking here.
    MaxEventTracker.Initialize(EventBackendFactory.Create(TrackingEventsType.Metica));
}
```

---

## Event Reference

### Ad Activity — `MaxEventTracker.AdActivity`

| Method | Description |
| --- | --- |
| `TrackRewardedAdOpportunity(string rewardedAdUnitId = null)` | A rewarded ad show opportunity has occurred. When you pass a `rewardedAdUnitId`, the event is only tracked when that rewarded ad is loaded and ready — so it is safe to call whenever you offer the placement. If no id is passed, the event is always tracked. |
| `TrackAdImpression()` | An ad was viewed by the user. |
| `TrackAdClicked()` | An ad was clicked by the user. |

```csharp
// Call whenever you offer the rewarded placement — pass the ad unit id and the readiness check is handled internally.
MaxEventTracker.AdActivity.TrackRewardedAdOpportunity(_rewardedAdUnitId);

// Inside your MAX ad callbacks
void OnAdDisplayed(string adUnitId, MaxSdkBase.AdInfo info) =>
    MaxEventTracker.AdActivity.TrackAdImpression();

void OnAdClicked(string adUnitId, MaxSdkBase.AdInfo info) =>
    MaxEventTracker.AdActivity.TrackAdClicked();
```

Under **Metica**, ads are shown through Metica, so call the same methods from Metica's ad callbacks instead of MAX's.
Hook up every ad format you use (interstitial, rewarded, banner, MREC):

```csharp
// Inside your Metica ad callbacks (shown for rewarded; repeat for the other formats you use)
MeticaAdsCallbacks.Rewarded.OnAdShowSuccess += ad =>
    MaxEventTracker.AdActivity.TrackAdImpression();

MeticaAdsCallbacks.Rewarded.OnAdClicked += ad =>
    MaxEventTracker.AdActivity.TrackAdClicked();
```

`TrackRewardedAdOpportunity(adUnitId)` needs no change: with the Metica backend its readiness check asks
`MeticaSdk.Ads.IsRewardedReady` instead of MAX.

**When to call `TrackRewardedAdOpportunity()`**

Call it at the exact moment the user is *presented* with a rewarded-ad button or prompt — i.e. when a UI element that offers a rewarded ad becomes visible — **not** when the ad is loaded, and **not** when the user taps it. It measures the *opportunity* (the offer being shown), so fire it once each time the offer appears. When you pass the placement's ad unit id, the built-in readiness check means it only reports when an ad is actually available, so you can safely call it every time the UI appears.

Typical moments:

- **A popup/window with a rewarded button opens** — e.g. a *Level Complete* screen showing "Watch an ad to double your coins", a *Revive?* prompt, a *Free Chest* / daily-reward dialog, or an *Out of Lives / Energy* dialog offering a rewarded refill.
- **A persistent rewarded button becomes visible** — e.g. an always-on *Hint*, *Booster*, *Undo*, or *Refresh* button in a puzzle game. Call it when that screen (and its button) is shown to the user; for a button that stays on screen, report it once per time the user reaches that screen rather than every frame.
- **A rewarded offer surfaces in the shop/store** — e.g. a "Watch to earn 50 gems" tile appears when the shop opens.

```csharp
// Level Complete screen just opened with a "Double your reward" button
private void ShowLevelCompletePopup()
{
    levelCompletePanel.SetActive(true);
    // The rewarded offer is now visible to the user:
    MaxEventTracker.AdActivity.TrackRewardedAdOpportunity(_rewardedAdUnitId);
}

// Puzzle screen with an always-visible "Hint" button just became active
private void OnPuzzleScreenOpened()
{
    MaxEventTracker.AdActivity.TrackRewardedAdOpportunity(_rewardedAdUnitId);
}
```

---

### Gaming — `MaxEventTracker.Gaming`

| Method | Description |
| --- | --- |
| `TrackLevelStart(int level)` | A user starts a new level. `level` is the level number. |
| `TrackLevelComplete(int level)` | A user successfully completes a level. `level` is the level number. |
| `TrackVirtualResourceTransaction(int value, string resourceType = null)` | A user spends or earns in-game virtual resources (not real money). See notes below. |
| `TrackTutorialComplete()` | A user completes the tutorial or onboarding process. |
| `TrackEnergyDepleted()` | A user's in-game energy reaches zero, preventing further gameplay. |
| `TrackUseProp()` | A user uses a prop or in-game tool such as a hint, refresh, undo, or booster. |
| `TrackGameShopEnter()` | A user opens or navigates to the in-game shop or store. |

**`TrackVirtualResourceTransaction` parameters:**

- **`value`** — the transaction amount. Report a **positive** value when the user *earns* resources, and a **negative** value when the user *spends* resources.
- **`resourceType`** *(optional)* — use one of the `GamingCategory` constants: `COINS`, `GEMS`, `ENERGY`, `HEARTS`, `XP`, `TOKENS`, `KEYS`, `MATERIALS`, `TICKETS`, `OTHER`.

```csharp
var gaming = MaxEventTracker.Gaming;

gaming.TrackLevelStart(7);
gaming.TrackLevelComplete(7);

// User spent 5 coins
gaming.TrackVirtualResourceTransaction(-5, GamingCategory.COINS);

// User earned 100 XP
gaming.TrackVirtualResourceTransaction(100, GamingCategory.XP);

gaming.TrackTutorialComplete();
gaming.TrackEnergyDepleted();
gaming.TrackUseProp();
gaming.TrackGameShopEnter();
```

---

### Generic — `MaxEventTracker.Generic`

| Method | Description |
| --- | --- |
| `TrackLogin()` | A user successfully logs into their account. |
| `TrackSignUp()` | A user completes the account registration or sign-up process. |
| *(automatic)* | The app-open event is tracked **automatically** by `Initialize()` — no manual call is required. |

```csharp
var generic = MaxEventTracker.Generic;

generic.TrackLogin();
generic.TrackSignUp();
// The app-open event is sent automatically during Initialize() — no manual call needed.
```

---

### Social — `MaxEventTracker.Social`

| Method | Description |
| --- | --- |
| `TrackAccountLinked()` | A user links a third-party or social account to their app profile. |
| `TrackFriendInvite()` | A user sends an invite to another person to join or interact with the app. |
| `TrackGiftTransaction()` | A user sends or receives a gift within the app. |
| `TrackGroupJoined()` | A user joins a group, team, or community within the app. |
| `TrackChatSent()` | A user sends a message via the app's chat or messaging feature. |
| `TrackPostView()` | A user views a social post or piece of user-generated content. |
| `TrackPostCreated()` | A user creates or uploads a new post or piece of content. |

```csharp
var social = MaxEventTracker.Social;

social.TrackAccountLinked();
social.TrackFriendInvite();
social.TrackGiftTransaction();
social.TrackGroupJoined();
social.TrackChatSent();
social.TrackPostView();
social.TrackPostCreated();
```

---

### Subscription — `MaxEventTracker.Subscription`

| Method | Description |
| --- | --- |
| `TrackSubscribe()` | A user subscribes to a paid plan or service. |
| `TrackStartTrial()` | A user starts a free trial of a subscription or premium feature. |

```csharp
var subscription = MaxEventTracker.Subscription;

subscription.TrackSubscribe();
subscription.TrackStartTrial();
```

---

## Full Example

```csharp
using GameDistrict.EventTracking;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private string _rewardedAdUnitId;

    private void Start()
    {
        MaxSdkCallbacks.OnSdkInitializedEvent += OnMaxInitialized;
        MaxSdk.SetSdkKey("YOUR_SDK_KEY");
        MaxSdk.InitializeSdk();
    }

    private void OnMaxInitialized(MaxSdkBase.SdkConfiguration config)
    {
        // Initialize event tracking once MAX is ready. This also tracks the app-open event.
        MaxEventTracker.Initialize(EventBackendFactory.Create(TrackingEventsType.Max));
    }

    // Later, anywhere in your game:
    private void OnRewardedOfferShown() =>
        MaxEventTracker.AdActivity.TrackRewardedAdOpportunity(_rewardedAdUnitId);

    private void OnLevelStarted(int level) =>
        MaxEventTracker.Gaming.TrackLevelStart(level);

    private void OnLevelCompleted(int level) =>
        MaxEventTracker.Gaming.TrackLevelComplete(level);

    private void OnCoinsSpent(int amount) =>
        MaxEventTracker.Gaming.TrackVirtualResourceTransaction(-amount, GamingCategory.COINS);
}
```

---

## Notes

- **Enable gate:** All tracking is a no-op until `Initialize()` is called. This is intentional so events are never sent before MAX is ready.
- **Logging:** Every tracked event writes a line to the Unity console for easy debugging, including its parameter values when any are supplied.
- **Log only what fits your game:** You don't need to call every event — track only the ones that match your game's actual features and moments. Not all events apply to every game, and logging irrelevant ones adds noise without value.
