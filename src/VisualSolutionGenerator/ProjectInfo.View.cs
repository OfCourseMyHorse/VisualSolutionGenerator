using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisualSolutionGenerator
{
    partial class ProjectInfo
    {
        public View CreateView(Collection c) { return View._Create(this, c); }

        [System.Diagnostics.DebuggerDisplay("{FilePath}")]
        public sealed class View : ProjectInfo
        {
            #region lifecycle

            internal static View _Create(ProjectInfo p, Collection c)
            {
                if (p == null) return null;
                if (c == null) return null;

                return new View(p, c);
            }

            private View(ProjectInfo pinfo, Collection c) : base(pinfo)
            {
                _Collection = c;
                _FallbackId = Guid.NewGuid();
            }

            public override FileBaseInfo Clone() { return new View(this); }


            private View(View other) : base(other)
            {
                _Collection = other._Collection;
                _FallbackId = other._FallbackId;
            }

            #endregion

            #region data

            internal Collection _Collection;

            private readonly Guid _FallbackId;

            #endregion

            #region properties

            public Guid EvaluatedProjectId
            {
                get
                {
                    var pg = _Project.GetPropertyValue("ProjectGuid");

                    return Guid.TryParse(pg, out Guid id) ? id : _Collection._GetDeferredProjectId(_Project.FullPath);
                }
            }

            public Guid ProjectId
            {
                get
                {
                    var pid = EvaluatedProjectId;
                    return pid == Guid.Empty ? _FallbackId : pid;
                }
            }

            /// <summary>
            /// Outgoing Project References
            /// </summary>
            public IEnumerable<ProjectInfo> ProjectReferences =>  _ResolvedProjectReferences.OfType<ProjectInfo>().ToList();

            public IEnumerable<ProjectInfo> TransitiveProjectReferences
            {
                get
                {
                    var references = _ResolvedProjectReferences
                        .OfType<ProjectInfo>()
                        .Select(item => item.CreateView(_Collection))
                        .ToList();

                    _TransitiveReduction(references);

                    return references;
                }
            }

            /// <summary>
            /// Incoming Project References
            /// </summary>
            public IEnumerable<ProjectInfo> ReferencedByProjects
            {
                get
                {
                    return _Collection
                        .ProjectFiles
                        .Where(prj => prj._ResolvedProjectReferences.Contains(this))
                        .ToList();
                }
            }

            #endregion

            #region API

            private static void _TransitiveReduction(IList<View> references)
            {
                foreach(var r in references.ToArray())
                {
                    if (references.Any(item => item.ProjectReferences.Contains(r))) references.Remove(r);
                }
            }

            #endregion
        }
    }
}
