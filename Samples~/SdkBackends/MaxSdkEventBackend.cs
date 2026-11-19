using System.Collections.Generic;

namespace GameDistrict.EventTracking
{
    /// <summary>
    /// <see cref="IEventBackend"/> for AppLovin MAX. Use it when the game initialized MAX directly.
    /// Create it through <see cref="EventBackendFactory"/> with <see cref="TrackingEventsType.Max"/>.
    /// <para>
    /// Lives in the game's code (Assembly-CSharp), not the package, so it compiles against MaxSdk however the
    /// project installed it (UPM package or a folder under Assets/).
    /// </para>
    /// </summary>
    public sealed class MaxSdkEventBackend : IEventBackend
    {
        public void TrackEvent(string eventName, IDictionary<string, string> parameters) =>
            MaxSdk.TrackEvent(eventName, parameters);

        public bool IsRewardedAdReady(string adUnitId) => MaxSdk.IsRewardedAdReady(adUnitId);
    }
}
