using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualSolutionGenerator
{    
    static class VisualStudioLocator
    {
        // https://github.com/microsoft/MSBuildLocator/issues 

        /// <summary>Query for all installed Visual Studio instances.</summary>
        public static IEnumerable<VisualStudioInstallation> QueryVisualStudioInstances()
        {
            yield break;

            // var instance = Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();

            // yield return new VisualStudioInstallation(instance.Name, instance.VisualStudioRootPath, instance.Version);
        }
    }
}
