using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace VisualSolutionGenerator
{
    using MSPROJECT = Microsoft.Build.Evaluation.Project;

    /// <summary>
    /// Base class for all Project files
    /// </summary>
    public abstract partial class FileBaseInfo : IEquatable<FileBaseInfo>
    {
        #region lifecycle

        public static FileBaseInfo Create(DirectoryInfo baseDirectoryPath, FileInfo filePath)
        {
            if (baseDirectoryPath == null) throw new ArgumentNullException(nameof(baseDirectoryPath));
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));

            // trying to solve Project version comparison:
            // http://stackoverflow.com/questions/20379359/msb4086-a-numeric-comparison-was-attempted
            // https://social.msdn.microsoft.com/Forums/vstudio/en-US/9fadc5c4-fcde-46e2-9df4-9900c6c70361/invalidprojectfileexception-when-trying-to-load-a-uwp-project?forum=msbuild
            // It's a BUG: https://github.com/Microsoft/msbuild/issues/778

            if (!Microsoft.Build.Locator.MSBuildLocator.IsRegistered) throw new InvalidOperationException("MSBuildLocator is not initialized.");

            try
            {
                var props = IsNetCoreProject(filePath) ? null : NetFrameworkProperties.Value;
                var project = new MSPROJECT(filePath.FullName, props, null);

                var pinfo = new FileProjectInfo(project);

                pinfo._SolutionView = SolutionGenerationHints.Factory(pinfo, baseDirectoryPath.FullName);

                return pinfo;
            }
            catch(Exception ex)
            {
                return new FileErrorInfo(filePath.FullName, ex);
            }
        }

        protected FileBaseInfo() { }

        public abstract FileBaseInfo Clone();

        #endregion

        #region data

        public abstract String FilePath { get; }

        public override int GetHashCode() { var f = FilePath; return f == null ? 0 : f.ToLower().GetHashCode(); }        

        public static bool AreEqual(FileBaseInfo a, FileBaseInfo b)
        {
            #pragma warning disable IDE0041 // Use 'is null' check
            if (Object.ReferenceEquals(a, b)) return true;
            if (Object.ReferenceEquals(a, null)) return false;
            if (Object.ReferenceEquals(b, null)) return false;
            #pragma warning restore IDE0041 // Use 'is null' check

            return String.Equals(a.FilePath, b.FilePath, StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(FileBaseInfo other) { return AreEqual(this, other); }

        public override bool Equals(object obj) { return AreEqual(this, obj as FileBaseInfo); }

        public static bool operator ==(FileBaseInfo a, FileBaseInfo b) { return AreEqual(a, b); }
        public static bool operator !=(FileBaseInfo a, FileBaseInfo b) { return !(a == b); }

        #endregion

        #region properties

        public string FileName => Path.GetFileNameWithoutExtension(FilePath);
        public string FileDirectory => Path.GetDirectoryName(FilePath);

        #endregion

        #region helpers

        private static readonly Lazy<IDictionary<string, string>> NetFrameworkProperties = new Lazy<IDictionary<string, string>>(() =>
        {
            var vsInstance = VisualStudioLocator.QueryVisualStudioInstances().OrderBy(vs => vs.Version).LastOrDefault();
            return vsInstance == null ? null :
                new Dictionary<string, string>
                {
                    ["MSBuildExtensionsPath"] = Path.Combine(vsInstance.VisualStudioRootPath, "MSBuild"),
                    ["MSBuildExtensionsPath32"] = Path.Combine(vsInstance.VisualStudioRootPath, "MSBuild"),
                    ["MSBuildExtensionsPath64"] = @"C:\Program Files\MSBuild"
                };
        });

        private static bool IsNetCoreProject(FileInfo projectPath)
        {
            // I would have expected ProjectRootElement to handle this, but in testing it doesn't extract a single property.
            var projXml = XDocument.Load(projectPath.FullName);
            var targetFramework =
                projXml.Descendants("TargetFramework").FirstOrDefault() ??
                projXml.Descendants("TargetFrameworks").FirstOrDefault() ??
                projXml.Descendants("TargetFrameworkVersion").FirstOrDefault();
            return (targetFramework?.Value?.Contains("core")).GetValueOrDefault();
        }

        #endregion
    }


}
