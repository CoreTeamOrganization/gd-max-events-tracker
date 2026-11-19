using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.UnityLinker;

namespace GameDistrict.EventTracking.EditorTools
{
    /// <summary>
    /// Hands the Unity linker a link.xml that preserves the whole <c>GameDistrict.MaxEventTracker</c>
    /// assembly, so none of its code is stripped at any Managed Stripping Level (Mono or IL2CPP), even
    /// when the game never calls it.
    /// <para>
    /// Needed because Unity only collects link.xml files from the Assets folder — a link.xml placed inside
    /// a package is ignored. <see cref="IUnityLinkerProcessor"/> is the supported way for a package to add one.
    /// </para>
    /// </summary>
    internal sealed class MaxEventTrackerLinkerProcessor : IUnityLinkerProcessor
    {
        private const string RuntimeAssemblyName = "GameDistrict.MaxEventTracker";

        private const string LinkXml =
            "<linker>\n" +
            "  <assembly fullname=\"" + RuntimeAssemblyName + "\" preserve=\"all\" />\n" +
            "</linker>\n";

        public int callbackOrder => 0;

        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            // Written under Library/ (project-local, not version-controlled) and regenerated every build.
            var directory = Path.GetFullPath(Path.Combine("Library", RuntimeAssemblyName));
            Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, "link.xml");
            File.WriteAllText(path, LinkXml);
            return path;
        }
    }
}
