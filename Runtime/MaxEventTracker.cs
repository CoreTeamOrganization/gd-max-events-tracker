using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameDistrict.EventTracking
{
    /// <summary>
    /// Entry point for MaxEventTracker. A plain static class: no prefab, no GameObject, nothing to place in a scene.
    /// <para>
    /// Static state is (re)set at <see cref="RuntimeInitializeLoadType.SubsystemRegistration"/>, the earliest
    /// runtime hook, before any scene loads. That gives every play session a clean, disabled state, including in
    /// the Editor with "Enter Play Mode Options" when domain reload is off and statics would otherwise survive
    /// between play sessions.
    /// </para>
    /// </summary>
    public static class MaxEventTracker
    {
        /// <summary>Package version.</summary>
        public const string Version = "0.3";

        /// <summary>True once <see cref="Initialize(IEventBackend)"/> has run this session.</summary>
        public static bool IsEnabled { get; private set; }

        /// <summary>The SDK bridge passed to <see cref="Initialize(IEventBackend)"/>; null until then.</summary>
        public static IEventBackend Backend { get; private set; }

        #region Event Groups

        // Created exactly once, when the class is first touched (also outside play mode, e.g. from editor code), so
        // they are never null. They hold no state (every call reads IsEnabled/Backend from this class), so they never
        // need recreating, not even between play sessions with domain reload off. A call made before
        // Initialize hits the IsEnabled guard in EventGroup.Track (a clear error), not a NullReferenceException.
        public static AdActivityEvents AdActivity { get; } = new AdActivityEvents();
        public static GamingEvents Gaming { get; } = new GamingEvents();
        public static GenericEvents Generic { get; } = new GenericEvents();
        public static SocialEvents Social { get; } = new SocialEvents();
        public static SubscriptionEvents Subscription { get; } = new SubscriptionEvents();

        #endregion

        /// <summary>
        /// Clears the per-session state at the start of every play session, so each session starts disabled.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            IsEnabled = false;
            Backend = null;

            lock (ReportedFailures)
                ReportedFailures.Clear();
        }

        // Backend calls that already threw this session, so each is logged only once.
        private static readonly HashSet<string> ReportedFailures = new HashSet<string>();

        /// <summary>
        /// Called when the SDK backend throws. Tracking must never break game code, so the exception is swallowed
        /// here. Each <paramref name="call"/> (e.g. "TrackEvent(level_start)") is logged once per session, with
        /// its stack trace. Later failures of the same call stay silent.
        /// </summary>
        internal static void ReportBackendFailure(string call, Exception exception)
        {
            lock (ReportedFailures)
            {
                if (!ReportedFailures.Add(call)) return;
            }

            Debug.LogError($"[MaxEventTracker] Backend {call} threw; the game carries on and further failures of " +
                           $"this call are not logged this session.\n{exception}");
        }

        /// <summary>
        /// Enables tracking and routes every event through <paramref name="backend"/>. Call it once the ad SDK
        /// has finished initializing. Also tracks the app-open event.
        /// </summary>
        /// <param name="backend">The game's SDK adapter (e.g. <c>EventBackendFactory.Create(TrackingEventsType.Max)</c> from the "SDK Backends" package sample).</param>
        public static void Initialize(IEventBackend backend)
        {
            if (backend == null)
            {
                Debug.LogError("[MaxEventTracker] Initialize called with a null backend — tracking stays disabled.");
                return;
            }

            if (IsEnabled)
            {
                Debug.LogWarning("[MaxEventTracker] Already initialized — ignoring duplicate Initialize call.");
                return;
            }

            Backend = backend;
            IsEnabled = true;
            Debug.Log($"[MaxEventTracker] Initialized v{Version}");

            Generic.TrackAppOpen();
        }
    }
}
