using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEngine;

namespace GameDistrict.EventTracking.EditorTools
{
    /// <summary>
    /// Keeps the <c>METICA_SDK_PRESENT</c> scripting define in sync with whether the Metica SDK is imported,
    /// so game code (<c>MeticaSdkEventBackend</c> from the "SDK Backends" sample) compiles only when Metica exists.
    /// <para>
    /// Detection looks for Metica's <c>Metica.SDK</c> assembly definition first. If that isn't found, it falls
    /// back to Metica's import folder, <c>Assets/MeticaSdk</c>. Metica ships only as a
    /// .unitypackage (no official UPM yet), so asmdef <c>versionDefines</c>, which only see UPM packages, can't
    /// detect it.
    /// </para>
    /// <para>
    /// This define only says Metica is <i>available</i>. Whether events actually go to Metica or AppLovin is
    /// decided at runtime: the game passes <c>TrackingEventsType.Max</c> or <c>.Metica</c> to
    /// <c>EventBackendFactory</c>, depending on which SDK it initialized (remote config may pick either in
    /// a release build).
    /// </para>
    /// <para>
    /// Re-checks after every script compilation, including one that failed. If Metica is deleted, game code
    /// still under the define fails to compile; this assembly doesn't depend on it, so it still runs, removes the
    /// define, and the next compile succeeds.
    /// </para>
    /// <para>
    /// Every define change is written to ProjectSettings.asset immediately, so it reaches version control and
    /// command-line builds without anyone saving the project. <see cref="MeticaDefineBuildCheck"/> also fails a
    /// build whose define doesn't match the project (e.g. a CI build of a checkout where the define was never
    /// saved), fixes the define, and asks for a rebuild.
    /// </para>
    /// </summary>
    [InitializeOnLoad]
    internal static class MeticaSdkDetector
    {
        public const string Define = "METICA_SDK_PRESENT";

        // Old manual toggle from the removed GameDistrict/MaxEventTracker/Use Metica menu; cleaned up if found.
        private const string LegacyDefine = "USE_METICA_TRACK_EVENTS";

        private const string MeticaAssemblyName = "Metica.SDK";

        private static readonly NamedBuildTarget[] Targets =
        {
            NamedBuildTarget.Android,
            NamedBuildTarget.iOS,
            NamedBuildTarget.Standalone,
        };

        static MeticaSdkDetector()
        {
            CompilationPipeline.compilationFinished += _ => Sync();
            EditorApplication.delayCall += Sync;
        }

        // Fallback: the folder the Metica .unitypackage imports into. Metica has no official UPM release yet, so a
        // folder under Assets/ is the only way it can be in a project.
        private const string MeticaFolder = "Assets/MeticaSdk";

        private const string StatusMenu = "GameDistrict/MaxEventTracker/Metica SDK Detected";
        private const string LoggedThisSessionKey = "GameDistrict.MaxEventTracker.MeticaStatusLogged";

        // Result of the last Sync, read by the menu checkmark (validate runs every time the menu opens).
        private static bool? _lastDetected;

        public static bool IsMeticaImported() => HasMeticaAssembly() || HasMeticaFolder();

        /// <summary>How Metica was found, or null if it wasn't.</summary>
        private static string DetectionSource() =>
            HasMeticaAssembly() ? $"{MeticaAssemblyName} assembly" :
            HasMeticaFolder() ? $"{MeticaFolder} folder" :
            null;

        // Primary: Metica's asmdef is registered (doesn't need it to have compiled).
        private static bool HasMeticaAssembly() =>
            CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies)
                .Any(a => a.name == MeticaAssemblyName);

        // Checked on disk (relative to the project root) rather than via AssetDatabase, which may not have
        // refreshed yet when this runs straight after a compile.
        private static bool HasMeticaFolder() => Directory.Exists(MeticaFolder);

        public static void Sync()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer) return;
            SyncNow();
        }

        /// <summary>Sync without the play-mode/build guard. Used by <see cref="MeticaDefineBuildCheck"/>.</summary>
        internal static void SyncNow()
        {
            string source = DetectionSource();
            bool present = source != null;
            _lastDetected = present;

            bool changed = false;
            foreach (var target in Targets)
                changed |= Apply(target, present);

            if (changed)
                SaveProjectSettings();

            // One status line per Editor session (SessionState survives domain reloads, clears on Editor restart).
            if (!SessionState.GetBool(LoggedThisSessionKey, false))
            {
                SessionState.SetBool(LoggedThisSessionKey, true);
                Debug.Log(present
                    ? $"[MaxEventTracker] Metica SDK detected ({source}) — {Define} is set; MeticaSdkEventBackend is available."
                    : $"[MaxEventTracker] Metica SDK not detected — {Define} is not set; EventBackendFactory uses AppLovin MAX.");
            }
        }

        // Indicator: a checkmark next to "Metica SDK Detected" means Metica is in the project.
        // Clicking shows the details (and re-checks first).
        [MenuItem(StatusMenu, priority = 0)]
        private static void ShowStatus()
        {
            Sync();
            string source = DetectionSource();

            string message = source != null
                ? $"Detected via the {source}.\n\n{Define} is set for Android, iOS and Standalone, so " +
                  "MeticaSdkEventBackend compiles and EventBackendFactory.Create(TrackingEventsType.Metica) uses Metica."
                : $"Not detected (no {MeticaAssemblyName} assembly and no {MeticaFolder} folder).\n\n{Define} is not " +
                  "set. EventBackendFactory.Create(TrackingEventsType.Metica) falls back to AppLovin MAX.";

            EditorUtility.DisplayDialog(source != null ? "Metica SDK: Detected" : "Metica SDK: Not detected",
                message, "OK");
        }

        [MenuItem(StatusMenu, validate = true, priority = 0)]
        private static bool ValidateShowStatus()
        {
            _lastDetected ??= IsMeticaImported();
            Menu.SetChecked(StatusMenu, _lastDetected.Value);
            return true;
        }

        internal static bool IsCoveredTarget(NamedBuildTarget target) => Targets.Contains(target);

        internal static bool HasDefine(NamedBuildTarget target) => GetDefines(target).Contains(Define);

        private static System.Collections.Generic.List<string> GetDefines(NamedBuildTarget target) =>
            PlayerSettings.GetScriptingDefineSymbols(target)
                .Split(';')
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .ToList();

        // Writes ProjectSettings.asset now. Otherwise a define change made by this script stays in memory until
        // something else saves the project, and a commit or CI build made in between misses it.
        private static void SaveProjectSettings()
        {
            var playerSettings = Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings");
            if (playerSettings != null)
                AssetDatabase.SaveAssetIfDirty(playerSettings);
            else
                AssetDatabase.SaveAssets();
        }

        /// <returns>True when the defines for <paramref name="target"/> were changed.</returns>
        private static bool Apply(NamedBuildTarget target, bool present)
        {
            var defines = GetDefines(target);

            bool changed = defines.Remove(LegacyDefine);

            if (present && !defines.Contains(Define)) { defines.Add(Define); changed = true; }
            else if (!present && defines.Remove(Define)) changed = true;

            if (!changed) return false;

            PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", defines));
            Debug.Log($"[MaxEventTracker] Metica SDK {(present ? "found" : "not found")} — " +
                      $"{Define} {(present ? "added" : "removed")} for {target.TargetName}.");
            return true;
        }
    }

    /// <summary>
    /// Fails a player build whose <c>METICA_SDK_PRESENT</c> define doesn't match whether Metica is in the project.
    /// Scripts are already compiled with the old defines by the time a build starts, so changing the define here
    /// can't save this build. Instead it fixes and saves the define, then stops the build with an explanation.
    /// The next build is correct.
    /// </summary>
    internal sealed class MeticaDefineBuildCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            var target = NamedBuildTarget.FromBuildTargetGroup(report.summary.platformGroup);
            if (!MeticaSdkDetector.IsCoveredTarget(target)) return;

            bool detected = MeticaSdkDetector.IsMeticaImported();
            bool defined = MeticaSdkDetector.HasDefine(target);
            if (detected == defined) return;

            MeticaSdkDetector.SyncNow();
            throw new BuildFailedException(
                $"[MaxEventTracker] The Metica SDK is {(detected ? "in the project" : "not in the project")}, but " +
                $"{MeticaSdkDetector.Define} was {(defined ? "set" : "not set")} for {target.TargetName}. The define " +
                $"has now been {(detected ? "added" : "removed")} and saved to ProjectSettings. Let scripts recompile, " +
                "then build again.");
        }
    }
}
