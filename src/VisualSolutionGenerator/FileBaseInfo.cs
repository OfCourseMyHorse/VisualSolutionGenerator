using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VisualSolutionGenerator
{
    using MSPROJECT = Microsoft.Build.Evaluation.Project;
    using MSPRJCOLLECTION = Microsoft.Build.Evaluation.ProjectCollection;

    /// <summary>
    /// Base class for all Project files
    /// </summary>
    public abstract partial class FileBaseInfo : IEquatable<FileBaseInfo>
    {
        #region lifecycle

        public static FileBaseInfo Create(System.IO.DirectoryInfo baseDirectoryPath, System.IO.FileInfo filePath)
        {
            // trying to solve Project version comparison:
            // http://stackoverflow.com/questions/20379359/msb4086-a-numeric-comparison-was-attempted
            // https://social.msdn.microsoft.com/Forums/vstudio/en-US/9fadc5c4-fcde-46e2-9df4-9900c6c70361/invalidprojectfileexception-when-trying-to-load-a-uwp-project?forum=msbuild
            // It's a BUG: https://github.com/Microsoft/msbuild/issues/778

            try
            {
                var project = new MSPROJECT(filePath.FullName);

                var pinfo = new ProjectInfo(project);

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

        public override bool Equals(object obj) { return Equals(this, obj as FileBaseInfo); }

        public static bool Equals(FileBaseInfo a, FileBaseInfo b)
        {
            #pragma warning disable IDE0041 // Use 'is null' check
            if (Object.ReferenceEquals(a, b)) return true;
            if (Object.ReferenceEquals(a, null)) return false;
            if (Object.ReferenceEquals(b, null)) return false;
            #pragma warning restore IDE0041 // Use 'is null' check

            return String.Equals(a.FilePath, b.FilePath, StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(FileBaseInfo other) { return Equals(this, other); }

        public static bool operator ==(FileBaseInfo a, FileBaseInfo b) { return Equals(a, b); }
        public static bool operator !=(FileBaseInfo a, FileBaseInfo b) { return !(a == b); }

        #endregion

        #region properties

        public string FileName => Path.GetFileNameWithoutExtension(FilePath);
        public string FileDirectory => Path.GetDirectoryName(FilePath);

        #endregion                
    }

    /// <summary>
    /// Project file that is missing or failed to load (corrupted?)
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{FilePath}")]
    public sealed class FileErrorInfo : FileBaseInfo
    {
        #region lifecycle

        internal FileErrorInfo(string filePath, Exception ex)
        {
            _FilePath = filePath;
            _Exception = ex;
        }

        private FileErrorInfo(FileErrorInfo other) : base()
        {
            _FilePath = other.FilePath;
            _Exception = other.Exception;
        }

        public override FileBaseInfo Clone() { return new FileErrorInfo(this); }

        #endregion

        #region data

        private readonly String _FilePath;
        private readonly Exception _Exception;

        #endregion

        #region properties

        public override String FilePath => _FilePath;

        public Exception Exception => _Exception;

        public String ExceptionName => _Exception.GetType().Name;
        public String ErrorMessage => _GetExMessage(_Exception);

        #endregion

        private static string _GetExMessage(Exception ex)
        {
            var msg = string.Empty;

            while(ex != null)
            {
                if (!string.IsNullOrWhiteSpace(msg)) msg += "\r\n";

                msg += ex.Message;
                ex = ex.InnerException;
            }

            return msg;
        }
    }
}
