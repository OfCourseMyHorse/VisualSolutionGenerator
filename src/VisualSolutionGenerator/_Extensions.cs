using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// this represents the source code DOM of the project file
using MSSRCPROJECT = Microsoft.Build.Construction.ProjectRootElement;

// this represents the evaluated data of the project file
using MSEVLPROJECT = Microsoft.Build.Evaluation.Project;

// this represent a project ready to build
using MSEXEPROJECT = Microsoft.Build.Execution.ProjectInstance;


namespace VisualSolutionGenerator
{
    // Accessing Project Type Specific Project, Project Item, and Configuration Properties
    // https://msdn.microsoft.com/en-us/library/ms228958.aspx

    static partial class _Extensions
    {
        private const string CSPROJCUSTOMPROP_VIRTUALFOLDERHINT = "SlnGenVirtualFolderHint";

        public static IEnumerable<String> GetProjectsReferencesRelativePaths(this MSEVLPROJECT proj)
        {
            return proj
                .AllEvaluatedItems
                .Where(item => item.ItemType == "Reference")
                .Select(item => item.EvaluatedInclude)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        public static IEnumerable<String> GetPackagesReferences(this MSEVLPROJECT proj)
        {
            return proj
                .AllEvaluatedItems
                .Where(item => item.ItemType == "PackageReference")
                .Select(item => item.EvaluatedInclude)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        public static IEnumerable<Guid> GetProjectTypes(this MSEVLPROJECT proj)
        {
            var guids = new HashSet<Guid>();

            var outt = proj.GetPropertyValue("UseWPF").ToLower();

            if (outt == "true") guids.Add(ProjectInfoTypes.WPF);

            var val = proj.GetPropertyValue("ProjectTypeGuids");
            if (!string.IsNullOrWhiteSpace(val))
            {
                var vals = val
                .Split(';')
                .Select(item => Guid.Parse(item));
                
                guids.UnionWith(vals);
            }

            var sorted = guids.ToList();
            sorted.Sort();
            
            return sorted;
                
        }

        public static String GetVirtualFolderHint(this MSEVLPROJECT proj) { return proj.GetPropertyValue(CSPROJCUSTOMPROP_VIRTUALFOLDERHINT); }

        public static AssemblyType GetAssemblyType(this MSEVLPROJECT proj)
        {
            var outt = proj.GetPropertyValue("OutputType").ToLower();

            var t = AssemblyType.None;

            if ( outt == "library" ) t |= AssemblyType.Library;
            if ( outt == "exe" || outt == "winexe" || outt == "appcontainerexe") t |= AssemblyType.Exe;

            if ( outt == "winexe") t |= AssemblyType.Win;
            if ( outt == "appcontainerexe") t |= AssemblyType.AppContainer;

            var packageReferences = proj.GetPackagesReferences().ToList();

            if (packageReferences.Contains("Microsoft.NET.Test.Sdk")) t |= AssemblyType.UnitTest;            

            // wix uses "package"
            // http://stackoverflow.com/questions/15555849/visual-studio-project-file-specify-multiple-import
            // if (outt == "package") ...

            if (proj.GetPropertyValue("AndroidApplication").ToLower() == "true") t |= AssemblyType.AppContainer; // android "App Projects" produce DLLs, not EXEs

            return t;
        }

        public static String[] GetTargetFrameworkMonikers(this MSEVLPROJECT proj)
        {
            // a project can target multiple frameworks:
            // https://blogs.msdn.microsoft.com/cesardelatorre/2016/06/28/running-net-core-apps-on-multiple-frameworks-and-what-the-target-framework-monikers-tfms-are-about/

            // target frameworks:
            // https://docs.microsoft.com/es-es/nuget/schema/target-frameworks

            var netstd = proj.GetPropertyValue("TargetFramework") + ";" + proj.GetPropertyValue("TargetFrameworks");
            netstd = netstd.Trim(';');

            if (!string.IsNullOrWhiteSpace(netstd)) return netstd.Split(';').OrderBy(item => item).ToArray();

            // fallback to legacy Net.Framework and create a compatible moniker

            

            if (proj.GetProjectTypes().Contains(ProjectInfoTypes.XAMARIN_ANDROID))
            {
                // note: in the list, android moniker is just "monoandroid" without version

                var tver = proj.GetPropertyValue("TargetFrameworkVersion").TrimStart('v').Replace(".", "");

                return new string[] { "android" + tver };
            }

            if (proj.GetProjectTypes().Contains(ProjectInfoTypes.IOS) || proj.GetProjectTypes().Contains(ProjectInfoTypes.IOS_OBSOLETE))
            {
                // note: in the list, android moniker is just "monoandroid" without version                

                return new string[] { "ios" };
            }

            if (proj.GetPropertyValue("TargetPlatformIdentifier") == "UAP")
            {
                var tver = proj.GetPropertyValue("TargetPlatformVersion").TrimStart('v');

                // tver = tver.Substring(0, tver.IndexOf('.'));

                return new string[] { "uap" + tver };
            }
            

            if (true)
            {
                var netframeworkVersion = proj.GetPropertyValue("TargetFrameworkVersion");
                var netframeworkProfile = proj.GetPropertyValue("TargetFrameworkProfile");

                var version = netframeworkVersion.TrimStart('v').Replace(".", "");
                var profile = netframeworkProfile.ToLower();

                if (profile.Contains("client")) version += "-client";  // Visual Studio standard moniquer: net35-client
                if (profile.StartsWith("profile"))
                {
                    // https://docs.microsoft.com/es-es/nuget/schema/target-frameworks
                    // Profile259 moniker = portable-net45+win8+wpa81+wp8                    

                    return new string[] { "portable-net" + version + "+" + profile.Substring(7) };
                }

                if (proj.GetProjectTypes().Contains(ProjectInfoTypes.PORTABLE_CLASS_LIBRARY) && version=="50")
                {
                    // this is a project originally created as PCL, converted to Net.Standard
                    return new string[] { "See Project.json" };
                }

                return new string[] { "net" + version };
            }            
        }

    }
}
