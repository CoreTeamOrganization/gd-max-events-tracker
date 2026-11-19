using System.Collections.Generic;

namespace GameDistrict.EventTracking
{
    /// <summary>
    /// The bridge between MaxEventTracker and the ad/analytics SDK that actually sends events.
    /// <para>
    /// The package never references an SDK directly, because projects install AppLovin MAX (and optionally
    /// Metica) in different ways — as a UPM package, or as a folder under Assets/. The game implements this
    /// interface in its own code, where the SDK is visible, and passes it to
    /// <see cref="MaxEventTracker.Initialize(IEventBackend)"/>. Ready-made AppLovin MAX and Metica adapters
    /// ship in the package sample "SDK Backends", created via <c>EventBackendFactory</c>.
    /// </para>
    /// </summary>
    public interface IEventBackend
    {
        /// <summary>Send a custom event, e.g. <c>MaxSdk.TrackEvent(eventName, parameters)</c>.</summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="parameters">Event parameters; never null (empty when the event has none).</param>
        void TrackEvent(string eventName, IDictionary<string, string> parameters);

        /// <summary>Whether a rewarded ad is loaded for the given ad unit, e.g. <c>MaxSdk.IsRewardedAdReady</c>.</summary>
        bool IsRewardedAdReady(string adUnitId);
    }
}
