namespace GameDistrict.EventTracking
{
    public class SocialEvents : EventGroup
    {
        internal SocialEvents() { }

        /// <summary>
        /// Trigger this event when a user links a third-party or social account to their app profile.
        /// </summary>
        public void TrackAccountLinked() => Track("account_linked");

        /// <summary>
        /// Trigger this event when a user sends an invite to another person to join or interact with the app.
        /// </summary>
        public void TrackFriendInvite() => Track("friend_invite");

        /// <summary>
        /// Trigger this event when a user sends or receives a gift within the app.
        /// </summary>
        public void TrackGiftTransaction() => Track("gift_transaction");

        /// <summary>
        /// Trigger this event when a user joins a group, team, or community within your app.
        /// </summary>
        public void TrackGroupJoined() => Track("group_joined");

        /// <summary>
        /// Trigger this event when a user sends a message via the app's chat or messaging feature.
        /// </summary>
        public void TrackChatSent() => Track("chat_sent");

        /// <summary>
        /// Trigger this event when a user views a social post or piece of user-generated content.
        /// </summary>
        public void TrackPostView() => Track("post_view");

        /// <summary>
        /// Trigger this event when a user creates or uploads a new post or piece of content.
        /// </summary>
        public void TrackPostCreated() => Track("post_created");
    }
}
