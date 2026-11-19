using UnityEngine.Scripting;

// Keep this package in player builds under every Managed Stripping Level (Mono and IL2CPP).
//
// AlwaysLinkAssembly: the linker processes this assembly even when nothing in the game references it
// (by default an unreferenced assembly is dropped entirely).
// Preserve on the assembly: marks every type in it as a root, so unused types/members are not stripped.
//
// The editor-side MaxEventTrackerLinkerProcessor additionally injects a link.xml with preserve="all"
// for this assembly — belt and braces, since link.xml files inside packages are not picked up by Unity
// on their own.
[assembly: AlwaysLinkAssembly]
[assembly: Preserve]
