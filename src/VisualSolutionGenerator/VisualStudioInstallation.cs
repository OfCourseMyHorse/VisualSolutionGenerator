using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualSolutionGenerator
{
    public class VisualStudioInstallation
    {
        internal VisualStudioInstallation(string name, string path, Version version)
        {
            Name = name;
            Version = version;
            VisualStudioRootPath = path;
        }

        public string Name { get; }
        public Version Version { get; }
        public string VisualStudioRootPath { get; }
    }
}
