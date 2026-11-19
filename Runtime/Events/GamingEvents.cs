using System.Collections.Generic;

namespace GameDistrict.EventTracking
{
    public class GamingEvents : EventGroup
    {
        internal GamingEvents() { }

        /// <summary>
        /// Trigger this event when a user starts a new level in your game.
        /// value → the level number
        /// </summary>
        public void TrackLevelStart(int level) =>
            Track("level_start", new Dictionary<string, string> { { "value", level.ToString() } });

        /// <summary>
        /// Trigger this event when a user successfully completes a level in your game.
        /// value → the level number
        /// </summary>
        public void TrackLevelComplete(int level) =>
            Track("level_complete", new Dictionary<string, string> { { "value", level.ToString() } });

        /// <summary>
        /// Trigger this event when a user spends or earns in-game virtual resources, such as coins, gems, or energy (not related to real money).
        /// value → the transaction amount. When the user earns in-game virtual resources, please report a positive value; when the user consumes / spends in-game virtual resources, please report a negative value.
        /// resource_type → the resource type. This field is optional. Use one of the <see cref="GamingCategory"/> constants
        /// (<see cref="GamingCategory.COINS"/>, <see cref="GamingCategory.GEMS"/>, <see cref="GamingCategory.ENERGY"/>,
        /// <see cref="GamingCategory.HEARTS"/>, <see cref="GamingCategory.XP"/>, <see cref="GamingCategory.TOKENS"/>,
        /// <see cref="GamingCategory.KEYS"/>, <see cref="GamingCategory.MATERIALS"/>, <see cref="GamingCategory.TICKETS"/>,
        /// <see cref="GamingCategory.OTHER"/>). Values are case insensitive.
        /// </summary>
        public void TrackVirtualResourceTransaction(int value, string resourceType = null)
        {
            var data = new Dictionary<string, string> { { "value", value.ToString() } };
            if (!string.IsNullOrEmpty(resourceType))
                data["resource_type"] = resourceType;
            Track("virtual_resource_transaction", data);
        }

        /// <summary>
        /// Trigger this event when a user completes the tutorial or onboarding process in your game.
        /// </summary>
        public void TrackTutorialComplete() => Track("tutorial_complete");

        /// <summary>
        /// Trigger this event when a user's in-game energy reaches zero, preventing further gameplay.
        /// </summary>
        public void TrackEnergyDepleted() => Track("energy_depleted");

        /// <summary>
        /// Trigger this event when a user uses a prop or in-game tool such as a hint, refresh, undo, or booster.
        /// </summary>
        public void TrackUseProp() => Track("use_prop");

        /// <summary>
        /// Trigger this event when a user opens or navigates to the in-game shop or store.
        /// </summary>
        public void TrackGameShopEnter() => Track("game_shop_enter");
    }
}
