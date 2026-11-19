#if METICA_SDK_PRESENT
using System.Collections.Generic;

namespace GameDistrict.EventTracking
{
    /// <summary>
    /// <see cref="IEventBackend"/> for Metica. Use it when the game initialized Metica, which drives MAX itself.
    /// Create it through <see cref="EventBackendFactory"/> with <see cref="TrackingEventsType.Metica"/>.
    /// <para>
    /// Compiled only while the Metica SDK is imported: the MaxEventTracker editor sets <c>METICA_SDK_PRESENT</c>
    /// automatically when it finds Metica's <c>Metica.SDK</c> assembly.
    /// </para>
    /// </summary>
    public sealed class MeticaSdkEventBackend : IEventBackend
    {
        // Pure passthrough to MAX's event service, via the MAX instance Metica drives.
        public void TrackEvent(string eventName, IDictionary<string, string> parameters) =>
            global::Metica.MeticaSdk.Ads.Max.TrackEvent(eventName, parameters);

        // Under Metica, ads are loaded through Metica, so ask Metica rather than the MAX Unity plugin.
        public bool IsRewardedAdReady(string adUnitId) => global::Metica.MeticaSdk.Ads.IsRewardedReady(adUnitId);
    }
}
#endif
