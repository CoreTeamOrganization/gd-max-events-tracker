using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameDistrict.EventTracking
{
    /// <summary>
    /// Base class for a grouped set of custom events. Handles the enabled check, logging, and
    /// forwarding to the game's <see cref="IEventBackend"/> (set in <see cref="MaxEventTracker.Initialize"/>).
    /// </summary>
    public abstract class EventGroup
    {
        // Created only by MaxEventTracker.
        private protected EventGroup() { }

        protected void Track(string eventName, IDictionary<string, string> parameters = null)
        {
            if (!MaxEventTracker.IsEnabled)
            {
                Debug.LogError($"[MaxEventTracker] Not initialized — failed to log '{eventName}' event.");
                return;
            }

            parameters ??= new Dictionary<string, string>();

            // Log the event name, plus its parameter values when any were supplied.
            if (parameters.Count > 0)
                Debug.Log($"[MaxEventTracker] {eventName} — {string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"))}");
            else
                Debug.Log($"[MaxEventTracker] {eventName}");

            // An SDK failure must never propagate into the caller's game code.
            try
            {
                MaxEventTracker.Backend.TrackEvent(eventName, parameters);
            }
            catch (Exception exception)
            {
                MaxEventTracker.ReportBackendFailure($"TrackEvent({eventName})", exception);
            }
        }
    }
}
