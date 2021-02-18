using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Setup.Configuration;

namespace VisualSolutionGenerator
{
    /// <summary>
    /// MSBuildLocator in a .Net Core application believes that the only MSBuild is the .Net Core SDK.
    /// This class allows you to locate the Visual Studio instances that MSBuildLocator would find if compiled for .Net Framework.
    /// </summary>
    static class VisualStudioLocator
    {
        /// <summary>Query for all installed Visual Studio instances.</summary>
        public static IEnumerable<VisualStudioInstallation> QueryVisualStudioInstances()
        {
            return Enumerable.Empty<VisualStudioInstallation>();

            const string MSBuild = "Microsoft.Component.MSBuild";

            var validInstances = new List<VisualStudioInstallation>();
            try
            {
                var iterator = (GetQuery() as ISetupConfiguration2).EnumAllInstances();
                while (true)
                {
                    var instances = new ISetupInstance[1];
                    // Call e.Next to query for the next instance (single item or nothing returned).
                    iterator.Next(1, instances, out int fetched);
                    if (fetched <= 0) break;

                    var instance = (ISetupInstance2)instances[0];
                    if (!Version.TryParse(instance.GetInstallationVersion(), out Version version))
                        continue;

                    // If the install was complete and a valid version, consider it.
                    InstanceState state = instance.GetState();
                    if (state == InstanceState.Complete || (state.HasFlag(InstanceState.Registered) && state.HasFlag(InstanceState.NoRebootRequired)))
                    {
                        if (instance.GetPackages().Any(pkg => string.Equals(pkg.GetId(), MSBuild, StringComparison.OrdinalIgnoreCase)))
                        {
                            validInstances.Add(new VisualStudioInstallation(instance.GetDisplayName(), instance.GetInstallationPath(), version));
                        }
                    }
                }
            }
            catch (COMException) { }
            catch (DllNotFoundException) { }

            return validInstances;
        }

        private static ISetupConfiguration GetQuery()
        {
            const int REGDB_E_CLASSNOTREG = unchecked((int)0x80040154);

            try { return new SetupConfiguration(); }
            catch (COMException ex) when (ex.ErrorCode == REGDB_E_CLASSNOTREG)
            {
                // Try to get the class object using app-local call.
                var result = GetSetupConfiguration(out ISetupConfiguration query, IntPtr.Zero);
                if (result < 0) throw new COMException($"Failed to get setup configuration", result);
                return query;
            }
        }

        [DllImport("Microsoft.VisualStudio.Setup.Configuration.Native.dll")]
        private static extern int GetSetupConfiguration([Out, MarshalAs(UnmanagedType.Interface)] out ISetupConfiguration configuration, IntPtr reserved);
    }    
}
