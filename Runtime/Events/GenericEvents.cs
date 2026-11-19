namespace GameDistrict.EventTracking
{
    public class GenericEvents : EventGroup
    {
        internal GenericEvents() { }

        /// <summary>
        /// Trigger this event when a user successfully logs into their account.
        /// </summary>
        public void TrackLogin() => Track("login");

        /// <summary>
        /// Trigger this event when a user completes the account registration or sign-up process.
        /// </summary>
        public void TrackSignUp() => Track("sign_up");

        /// <summary>
        /// Trigger this event when the user opens or launches your app.
        /// Called automatically from <see cref="MaxEventTracker.Initialize"/>.
        /// </summary>
        internal void TrackAppOpen() => Track("app_open");
    }
}
