namespace GameDistrict.EventTracking
{
    public class SubscriptionEvents : EventGroup
    {
        internal SubscriptionEvents() { }

        /// <summary>
        /// Trigger this event when a user subscribes to a paid plan or service in your app.
        /// </summary>
        public void TrackSubscribe() => Track("subscribe");

        /// <summary>
        /// Trigger this event when a user starts a free trial of a subscription or premium feature.
        /// </summary>
        public void TrackStartTrial() => Track("start_trial");
    }
}
