using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisualSolutionGenerator
{
    partial class FileBaseInfo
    {
        public sealed class Collection
        {
            #region lifecycle

            public Collection(System.IO.DirectoryInfo dir)
            {
                _BaseDirectory = dir;
            }

            #endregion

            #region constants

            private const string ALLPROJS = "*" + FileProjectInfo.ProjFileExtension;

            #endregion

            #region data

            private readonly System.IO.DirectoryInfo _BaseDirectory;

            private readonly List<FileBaseInfo> _Files = new List<FileBaseInfo>();

            // some projects (net.standard) might miss a project GUID, so we need to resolve them in a deferred way
            private readonly Dictionary<String, Guid> _ProjectGuids = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);            

            #endregion

            #region properties

            public IEnumerable<FileProjectInfo.View> ProjectFiles => _Files.OfType<FileProjectInfo>().Select(item => item.CreateView(this)).ToList();

            public IEnumerable<FileErrorInfo> FailedFiles => _Files.OfType<FileErrorInfo>().ToList();

            #endregion

            #region API            

            private void _RegisterDeferredProjectId(string fullPath, Guid id)
            {
                if (id == Guid.Empty) return;

                if (_GetDeferredProjectId(fullPath) != Guid.Empty) return; // todo, check id is the same

                _ProjectGuids[fullPath] = id;
            }

            internal Guid _GetDeferredProjectId(string fullPath)
            {
                return _ProjectGuids.TryGetValue(fullPath, out Guid id) ? id : Guid.Empty;
            }

            public FileProjectInfo GetByGuid(Guid pid)
            {
                return pid == Guid.Empty ? null : ProjectFiles.FirstOrDefault(p => p.ProjectId == pid);
            }

            public FileProjectInfo UseProject(System.IO.FileInfo finfo)
            {
                if (finfo == null || !finfo.Exists) return null;

                var fkey = finfo.FullName.ToLowerInvariant();

                var existing = _Files.FirstOrDefault(item => item.FilePath.ToLower() == fkey);

                if (existing != null) return existing as FileProjectInfo;

                // load the project and add it to the collection

                var xinfo = FileBaseInfo.Create(_BaseDirectory, finfo);

                if (xinfo is FileErrorInfo) { _Files.Add(xinfo); return null; }

                var pinfo = xinfo as FileProjectInfo;

                /*
                if (pinfo.ProjectId != Guid.Empty)
                {
                    var sameGuidPrj = GetByGuid(pinfo.ProjectId);
                    if (sameGuidPrj != null) { System.Diagnostics.Debug.Fail(String.Format("WARNING: Projects [{0}] and [{1}] share the same GUID", pinfo.ProjectId, sameGuidPrj.ProjectId)); }
                }*/               

                //-------------------------------
                _Files.Add(pinfo);
                //-------------------------------

                pinfo._EvaluateReferences
                    (
                    (rfinfo,pid) =>
                        {
                            _RegisterDeferredProjectId(rfinfo.FullName, pid);

                            return UseProject(rfinfo);
                        }
                    );

                return pinfo;
            }

            public void ProbeFolder(string folder, IEnumerable<string> excludeProjs, IEnumerable<string> excludeDirs, bool recursive = false)
            {
                var xfolder = new System.IO.DirectoryInfo(folder);                

                var xexcludeDirs = excludeDirs?
                    .Select(d => new System.IO.DirectoryInfo(d))
                    .Where(d => d.Exists)
                    .ToArray();

                ProbeFolder(xfolder, excludeProjs, xexcludeDirs, recursive);
            }

            public void ProbeFolder(System.IO.DirectoryInfo folder, IEnumerable<string> excludeProjs, IEnumerable<System.IO.DirectoryInfo> excludeDirs, bool recursive = false)
            {
                if (!folder.Exists) return;                

                if (excludeDirs != null && excludeDirs.Any(exdir => exdir.FullName == folder.FullName)) return;

                foreach (var f in folder.GetFiles(ALLPROJS))
                {
                    // if (excludeProjs != null && excludeProjs.Any(exprj => System.IO.Path.GetFileName(f).ToLower() == exprj.ToLower() ) ) continue;

                    UseProject(f);
                }

                if (!recursive) return;

                foreach (var d in folder.GetDirectories())
                {
                    ProbeFolder(d, excludeProjs, excludeDirs, true);
                }
            }

            #endregion

            #region export DGML

            public OpenSoftware.DgmlTools.Model.DirectedGraph ToDGML()
            {
                // https://github.com/merijndejonge/DgmlBuilder        

                var projects = ProjectFiles.Where(item => item != null).ToList();

                var linkPairs = projects
                    .SelectMany(item => item.TransitiveProjectReferences.Select(iref => new KeyValuePair<FileProjectInfo, FileBaseInfo>(item, iref)))
                    .Cast<Object>()
                    .ToList();

                var groups = projects
                    .Select(item => item.Solution.VirtualFolderPath)
                    .Distinct()
                    .ToList();

                var groupBuilder = new OpenSoftware.DgmlTools.Builders.NodeBuilder<String>
                    (
                    x => new OpenSoftware.DgmlTools.Model.Node
                    {
                        Id = x,
                        Label = x,
                        Group = "Expanded"
                    }
                    );

                var groupItemBuilder = new OpenSoftware.DgmlTools.Builders.LinkBuilder<FileProjectInfo>
                    (
                    x => new OpenSoftware.DgmlTools.Model.Link
                    {
                        Category = "Contains",
                        Source = x.Solution.VirtualFolderPath,
                        Target = x.FilePath                        
                    });

                var prjBuilder = new OpenSoftware.DgmlTools.Builders.NodeBuilder<FileProjectInfo>
                    (
                    x => new OpenSoftware.DgmlTools.Model.Node
                    {
                        Id = x.FilePath,
                        Label = x.AssemblyName,
                        Reference = x.FilePath
                    }
                    );

                var errBuilder = new OpenSoftware.DgmlTools.Builders.NodeBuilder<FileErrorInfo>
                    (
                    x => new OpenSoftware.DgmlTools.Model.Node
                    {
                        Id = x.FilePath,
                        Label = x.FileName,
                        Reference = x.FilePath
                    }
                    );

                var linkBuilder = new OpenSoftware.DgmlTools.Builders.LinkBuilder<KeyValuePair<FileProjectInfo, FileBaseInfo>>
                    (
                    x => new OpenSoftware.DgmlTools.Model.Link
                    {
                        Target = x.Key.FilePath,
                        Source = x.Value.FilePath
                    });

                var builder = new OpenSoftware.DgmlTools.DgmlBuilder
                {
                    NodeBuilders = new OpenSoftware.DgmlTools.Builders.NodeBuilder[] { prjBuilder,errBuilder , groupBuilder },
                    LinkBuilders = new OpenSoftware.DgmlTools.Builders.LinkBuilder[] { linkBuilder, groupItemBuilder },
                    CategoryBuilders = new OpenSoftware.DgmlTools.Builders.CategoryBuilder[] { },
                    StyleBuilders = new OpenSoftware.DgmlTools.Builders.StyleBuilder[] { }
                };

                return builder.Build(groups, projects, linkPairs);
            }

            #endregion
        }
    }
}
