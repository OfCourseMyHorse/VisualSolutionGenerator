using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualSolutionGenerator
{
    public class PackageInfo
    {
        public PackageInfo(string n, string v)
        {
            AssemblyName = n;
            VersionInfo = v;
        }

        public string AssemblyName { get; set; }
        public string VersionInfo { get; set; }
    }
}
