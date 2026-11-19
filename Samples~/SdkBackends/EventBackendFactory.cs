using UnityEngine;

namespace GameDistrict.EventTracking
{
    /// <summary>
    /// Creates the <see cref="IEventBackend"/> for the SDK the game initialized this session.
    /// Call it from that SDK's "initialized" callback and pass the result to MaxEventTracker:
    /// <code>
    /// // AppLovin MAX finished initializing:
    /// MaxEventTracker.Initialize(EventBackendFactory.Create(TrackingEventsType.Max));
    ///
    /// // Metica finished initializing:
    /// MaxEventTracker.Initialize(EventBackendFactory.Create(TrackingEventsType.Metica));
    /// </code>
    /// </summary>
    public static class EventBackendFactory
    {
        /// <returns>
        /// The backend for <paramref name="type"/>. If Metica is requested but the Metica SDK isn't imported, logs a
        /// warning and returns the AppLovin MAX backend. Returns null only for an unknown type.
        /// </returns>
        public static IEventBackend Create(TrackingEventsType type)
        {
            switch (type)
            {
                case TrackingEventsType.Max:
                    return new MaxSdkEventBackend();

                case TrackingEventsType.Metica:
#if METICA_SDK_PRESENT
                    return new MeticaSdkEventBackend();
#else
                    Debug.LogWarning("[MaxEventTracker] TrackingEventsType.Metica requested, but the Metica SDK isn't " +
                                     "imported (METICA_SDK_PRESENT is not defined). Falling back to AppLovin MAX.");
                    return new MaxSdkEventBackend();
#endif

                default:
                    Debug.LogError($"[MaxEventTracker] Unknown TrackingEventsType '{type}'. No backend created.");
                    return null;
            }
        }
    }
}
