namespace GameDistrict.EventTracking
{
    public class AdActivityEvents : EventGroup
    {
        internal AdActivityEvents()
        {
        }

        /// <summary>
        /// Trigger this event when a rewarded ad show opportunity has occurred in the app — i.e. the moment
        /// a rewarded-ad button/prompt is presented to the user (a Level Complete "double your reward" popup,
        /// a revive/refill dialog, or an always-visible hint/booster button in a puzzle game). Call it when
        /// the offer becomes visible — not when the ad loads, and not when the user taps it — once per time
        /// the offer appears.
        /// When a <paramref name="rewardedAdUnitId"/> is supplied, the event is only tracked if that
        /// rewarded ad is loaded and ready to show. If no ad unit id is supplied, the event is always tracked.
        /// </summary>
        /// <param name="rewardedAdUnitId">
        /// The rewarded ad unit id to check readiness against. Pass the ad unit for the placement you are
        /// offering; leave null/empty to always track the opportunity without a readiness check.
        /// </param>
        public void TrackRewardedAdOpportunity(string rewardedAdUnitId = null)
        {
            // With no ad unit id supplied we can't check readiness — just track the event. Before
            // Initialize there is no backend; Track then reports the "not initialized" error.
            if (string.IsNullOrEmpty(rewardedAdUnitId) ||
                MaxEventTracker.Backend == null ||
                IsRewardedAdReady(rewardedAdUnitId))
                Track("rewarded_ad_opportunity");
        }

        // If the SDK throws, treat the ad as not ready (skip the event) rather than let the exception reach the
        // caller's game code.
        private static bool IsRewardedAdReady(string rewardedAdUnitId)
        {
            try
            {
                return MaxEventTracker.Backend.IsRewardedAdReady(rewardedAdUnitId);
            }
            catch (System.Exception exception)
            {
                MaxEventTracker.ReportBackendFailure($"IsRewardedAdReady({rewardedAdUnitId})", exception);
                return false;
            }
        }

        /// <summary>
        /// Trigger this event when an ad is viewed by the user.
        /// </summary>
        public void TrackAdImpression() => Track("ad_view");

        /// <summary>
        /// Trigger this event when an ad was clicked by the user.
        /// </summary>
        public void TrackAdClicked() => Track("ad_clicked");
    }
}
