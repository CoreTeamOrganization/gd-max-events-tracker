namespace GameDistrict.EventTracking
{
    /// <summary>
    /// Which SDK MaxEventTracker sends its events through. Pass it to the game's
    /// <c>EventBackendFactory.Create</c> (from the "SDK Backends" package sample) when that SDK has
    /// finished initializing. A session initializes exactly one of them.
    /// </summary>
    public enum TrackingEventsType
    {
        /// <summary>AppLovin MAX: <c>MaxSdk.TrackEvent</c>.</summary>
        Max,

        /// <summary>Metica: <c>MeticaSdk.Ads.Max.TrackEvent</c>. Requires the Metica SDK to be imported.</summary>
        Metica,
    }
}
