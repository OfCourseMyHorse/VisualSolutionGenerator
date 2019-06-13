using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualSolutionGenerator
{
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

        #region core

        private static string _GetExMessage(Exception ex)
        {
            var msg = string.Empty;

            while (ex != null)
            {
                if (!string.IsNullOrWhiteSpace(msg)) msg += "\r\n";

                msg += ex.Message;
                ex = ex.InnerException;
            }

            return msg;
        }

        #endregion
    }
}
