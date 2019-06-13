using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace VisualSolutionGenerator
{
    [System.Diagnostics.DebuggerDisplay("{Name} {FolderId}")]
    sealed class _SolutionVirtualFolder
    {
        #region lifecycle

        /// <summary>
        /// Creates a virtual folder view within the solution
        /// </summary>
        /// <param name="projects">projects to be contained in the virtual folder</param>
        /// <returns></returns>
        public static _SolutionVirtualFolder CreateRoot(IEnumerable<FileProjectInfo.View> projects)
        {
            var root = new _SolutionVirtualFolder(null);

            foreach (var prj in projects)
            {
                root.Use(prj);
            }

            return root;
        }

        private _SolutionVirtualFolder(string name)
        {
            Name = name;
            FolderId = string.IsNullOrWhiteSpace(name) ? Guid.Empty : Guid.NewGuid();
        }        

        #endregion

        #region data

        [System.Diagnostics.DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public _SolutionVirtualFolder Parent { get; set; }

        [System.Diagnostics.DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly List<_SolutionVirtualFolder> _Children = new List<_SolutionVirtualFolder>();

        [System.Diagnostics.DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly HashSet<Guid> _Projects = new HashSet<Guid>();

        #endregion

        #region properties

        public string Name { get; set; }

        public Guid FolderId { get; private set; }

        [System.Diagnostics.DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Guid ParentId => Parent == null ? Guid.Empty : Parent.FolderId;        

        #endregion

        #region API

        private _SolutionVirtualFolder _UseChild(string aliasFolder)
        {
            if (string.IsNullOrWhiteSpace(aliasFolder)) return null;
            aliasFolder = aliasFolder.Trim();

            var f = _Children.FirstOrDefault(item => item.Name == aliasFolder);
            if (f != null) return f;

            f = new _SolutionVirtualFolder(aliasFolder)
            {
                Parent = this
            };

            _Children.Add(f);

            return f;
        }

        private _SolutionVirtualFolder _Use(string aliasPath)
        {
            if (string.IsNullOrWhiteSpace(aliasPath)) return null;

            var parts = aliasPath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0) return null;

            var child = _UseChild(parts[0]);

            if (parts.Length == 1) return child;

            var subPath = string.Join(Path.DirectorySeparatorChar.ToString(), parts, 1, parts.Length - 1);

            return child._Use(subPath);
        }

        public void Use(FileProjectInfo.View pinfo)
        {
            if (string.IsNullOrWhiteSpace(pinfo.Solution.VirtualFolderPath)) return;

            var folder = _Use(pinfo.Solution.VirtualFolderPath);

            folder._Projects.Add(pinfo.ProjectId);
        }

        public bool ContainsProject(FileProjectInfo.View pinfo) { return _Projects.Contains(pinfo.ProjectId); }

        public Guid GetVirtualFolderId(FileProjectInfo.View pinfo)
        {
            if (ContainsProject(pinfo)) return FolderId;

            foreach (var f in _Children)
            {
                var r = f.GetVirtualFolderId(pinfo);
                if (r != Guid.Empty) return r;
            }

            return Guid.Empty;
        }

        [System.Diagnostics.DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IEnumerable<_SolutionVirtualFolder> AllFolders
        {
            get
            {
                return _Children.Concat(_Children.SelectMany(item => item.AllFolders)).Distinct();
            }
        }

        #endregion
    }
}
