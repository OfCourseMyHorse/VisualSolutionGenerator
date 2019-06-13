#region Copyright MIT License
/*
 * Copyright © 2008 François St-Arnaud and John Wood
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * SolGen, Visual Studio Solution Generator for C# Projects (http://codeproject.com/SolGen)
 * Based on original work by John Wood developer of Priorganizer (http://www.priorganizer.com)
 * Adapted for Visual Studio 2005 by François St-Arnaud (francois.starnaud@videotron.ca)
 * Published to CodePlex with original author's permission.
 * 
 * Original software copyright notice:
 * © Copyright 2005 J. Wood Software Services. All rights reserved.
 * You are free to modify this source code at will, but please give credit to John Wood
 * if decide to incorporate or redistribute the resultant binary.
 * http://www.servicestuff.com/jwss/page.aspx?page=utility.htm&utility=solgen
 * 
 */
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Reflection;
using System.Linq;

// this represents the source code DOM of the project file
using MSSRCPROJECT = Microsoft.Build.Construction.ProjectRootElement;

// this represents the evaluated data of the project file
using MSEVLPROJECT = Microsoft.Build.Evaluation.Project;

// this represent a project ready to build
using MSEXEPROJECT = Microsoft.Build.Execution.ProjectInstance;


namespace VisualSolutionGenerator
{
    [Flags]
    public enum AssemblyType { None = 0, Library = 1, Exe = 2, AppContainer = 4, Win = 8 }

    public sealed class SolutionGenerationHints
    {
        #region lifecycle

        public static SolutionGenerationHints Factory(FileProjectInfo pinfo, string baseDirectoryPath)
        {
            if (pinfo == null) return null;
            // if (pinfo.ProjectId == Guid.Empty) return null;

            var vdir = System.IO.Path.GetDirectoryName(pinfo.FileDirectory);

            if (!baseDirectoryPath.EndsWith("\\")) baseDirectoryPath += "\\";

            if (vdir.StartsWith(baseDirectoryPath, StringComparison.OrdinalIgnoreCase))
            {
                vdir = vdir.Substring(baseDirectoryPath.Length);
            }
            else
            {
                vdir = System.IO.Path.GetFileName(vdir);
            }

            var hintFromProjectProperty = pinfo.MsProject.GetVirtualFolderHint();
            if (!string.IsNullOrWhiteSpace(hintFromProjectProperty)) { vdir = hintFromProjectProperty; }

            var sd = new SolutionGenerationHints
            {
                VirtualFolderPath = vdir
            };

            return sd;
        }

        private SolutionGenerationHints() { }

        #endregion

        #region properties

        // this is the virtual folder within the solution
        public string VirtualFolderPath { get; set; }        

        #endregion
    }

    [System.Diagnostics.DebuggerDisplay("{_Path}")]
    public sealed class ProjectReference : IEquatable<ProjectReference>
    {
        #region lifecycle

        public ProjectReference(Guid id, String path) { _Id = id;_Path = path; }

        #endregion

        #region data

        private readonly Guid _Id;
        private readonly String _Path;

        public override int GetHashCode()
        {
            if (_Id == Guid.Empty && string.IsNullOrWhiteSpace(_Path)) return 0;

            if (_Id != Guid.Empty) return _Id.GetHashCode();

            return _Path.ToLower().GetHashCode();
        }

        public static bool Equals(ProjectReference a, ProjectReference b)
        {
            #pragma warning disable IDE0041 // Use 'is null' check
            if (object.ReferenceEquals(a, b)) return true;
            if (object.ReferenceEquals(a, null)) return false;
            if (object.ReferenceEquals(b, null)) return false;
            #pragma warning restore IDE0041 // Use 'is null' check

            if (a._Id != Guid.Empty && b._Id != Guid.Empty) return a._Id == b._Id;

            return string.Equals(a._Path, b._Path, StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(ProjectReference other) => Equals(this, other);

        public override bool Equals(object obj) => Equals(this, obj as ProjectReference);

        public static bool operator ==(ProjectReference a, ProjectReference b) => Equals(a, b);
        public static bool operator !=(ProjectReference a, ProjectReference b) => !Equals(a, b);

        #endregion
    }    

    [System.Diagnostics.DebuggerDisplay("{FilePath}")]
    public partial class FileProjectInfo : FileBaseInfo
    {
        #region Constants
        
        private const string CSPROJCUSTOMPROP_CATEGORIES = "SlnGenCategories";

        protected const string ProjectRefs = ".projRefs";
        protected const string AssemblyRefs = ".dllRefs";
        public    const string ProjFileExtension = ".*proj"; // shproj csproj cxxproj  targets projitems
        protected const string SolutionFileExtension = ".sln";
        protected const string AllProjectsSolutionFileName = "all";

        protected const string ALLPROJS = "*" + ProjFileExtension;
        protected const string ALLFILES = "*.*";
        protected const string ALLDLLS = "*.dll";
        protected const string ALLSLNS = "*.sln";
        protected const string REFERENCE = "Reference";
        protected const string PROJECTREFERENCE = "ProjectReference";
        protected const string PACKAGEREFERENCE = "PackageReference";
        protected const string PROJECT = "Project";
        protected const string Obj = @"\obj\";

        #endregion

        #region lifecycle        

        internal FileProjectInfo(MSEVLPROJECT prj)
        {
            _Project = prj;            
        }

        public override FileBaseInfo Clone() { return new FileProjectInfo(this); }

        protected FileProjectInfo(FileProjectInfo other) : base()
        {
            this._Project = other._Project;            

            this._SolutionView = other._SolutionView;
            
            this._ResolvedProjectReferences.UnionWith(other._ResolvedProjectReferences);
        }
        

        /// <summary>
        /// for each referenced project, we recursively load the projects referenced into our collection
        /// </summary>
        /// <remarks>
        /// While we load the references, we "connect" the projects
        /// </remarks>
        /// <param name="evalRefFunc"></param>
        internal void _EvaluateReferences( Func<FileInfo, Guid, FileProjectInfo> evalRefFunc )
        {
            foreach (var buildItem in this._Project.AllEvaluatedItems)
            {
                if (buildItem.ItemType != PROJECTREFERENCE) continue;

                try
                {
                    var projGuid = Guid.Empty; Guid.TryParse(buildItem.GetMetadataValue(PROJECT), out projGuid);
                    var projPath = buildItem.EvaluatedInclude;

                    if (!System.IO.Path.IsPathRooted(projPath)) projPath = System.IO.Path.Combine(FileDirectory, projPath);
                    var projFnfo = new System.IO.FileInfo(projPath);                    

                    var rp = evalRefFunc(projFnfo, projGuid); if (rp == null) continue;
                    _ResolvedProjectReferences.Add(rp);
                }
                catch (Exception ex)
                {
                    // _LocalLog.Add(ex.ToString());
                }
            }
        }

        #endregion

        #region data
        
        private readonly MSEVLPROJECT _Project;
        
        internal SolutionGenerationHints _SolutionView;        

        protected readonly HashSet<FileBaseInfo> _ResolvedProjectReferences = new HashSet<FileBaseInfo>();

        #endregion

        #region properties - All

        public MSEVLPROJECT         MsProject           => _Project;

        public override string      FilePath            => _Project.FullPath;

        public string               ToolsVersion        => _Project.ToolsVersion;        

        public IEnumerable<Guid>    ProjectTypeGuids    => _Project.GetProjectTypes();

        public string               ProjectTypes        => string.Join(", ", ProjectTypeGuids.Select(item => item.GetProjectTypeDescription()).ToArray());

        #endregion

        #region properties - project core                

        public String       AssemblyName    =>  _Project.GetPropertyValue("AssemblyName");
        public AssemblyType AssemblyType    => _Project.GetAssemblyType();

        public String OutputType            =>  _Project.GetPropertyValue("OutputType");
        
        public Boolean IsApplication        => AssemblyType != AssemblyType.None;

        public String TargetFrameworks      => String.Join(" ", _Project.GetTargetFrameworkMonikers());

        public IEnumerable<PackageInfo> PackageReferences => this._Project.AllEvaluatedItems.Where(item => item.ItemType == PACKAGEREFERENCE).Select(item => new PackageInfo(item.EvaluatedInclude, item.GetMetadataValue("Version"))).ToList();

        #endregion        

        #region properties - extras        

        // hints for solution generation (move to view)
        public SolutionGenerationHints      Solution        => _SolutionView;

        public ProjectAnalysis              Analysis        => new ProjectAnalysis(_Project);

        public System.Xml.Linq.XDocument    FileContent     => System.Xml.Linq.XDocument.Load(this.FilePath);

        public IEnumerable<Microsoft.Build.Evaluation.ProjectItem> ItemsToCompile => _Project.GetItems("Compile").ToList();        

        #endregion        
    }
    
    [System.Diagnostics.DebuggerDisplay("{FilePath}")]
    public sealed class ProjectAnalysis : BindableBase
    {
        #region lifecycle

        internal ProjectAnalysis(MSEVLPROJECT prj) { _Project = prj; }

        #endregion

        #region Data

        internal readonly MSEVLPROJECT _Project;

        private bool _ShowPropertiesEnvironment;
        private bool _ShowPropertiesGlobal;
        private bool _ShowPropertiesReserved;
        private bool _ShowPropertiesInported;
        private string _ShowPropertiesContaining;        

        #endregion

        #region properties

        public bool ShowPropertiesEnvironment { get { return _ShowPropertiesEnvironment; } set { _ShowPropertiesEnvironment = value; RaiseChanged(nameof(Properties)); } }
        public bool ShowPropertiesGlobal { get { return _ShowPropertiesGlobal; } set { _ShowPropertiesGlobal = value; RaiseChanged(nameof(Properties)); } }
        public bool ShowPropertiesReserved { get { return _ShowPropertiesReserved; } set { _ShowPropertiesReserved = value; RaiseChanged(nameof(Properties)); } }
        public bool ShowPropertiesInported { get { return _ShowPropertiesInported; } set { _ShowPropertiesInported = value; RaiseChanged(nameof(Properties)); } }
        public string ShowPropertiesContaining { get { return _ShowPropertiesContaining; } set { _ShowPropertiesContaining = value; RaiseChanged(nameof(Properties)); } }        

        public IEnumerable<Tuple<string, string, string>> Properties
        {
            get
            {
                var props = new List<Tuple<string,string,string>>();

                foreach (var p in _Project.AllEvaluatedProperties)
                {
                    var filter = false;

                    if (_ShowPropertiesEnvironment && p.IsEnvironmentProperty) filter |= true;
                    if (_ShowPropertiesGlobal && p.IsGlobalProperty) filter |= true;
                    if (_ShowPropertiesReserved && p.IsReservedProperty) filter |= true;
                    if (_ShowPropertiesInported && p.IsImported) filter |= true;

                    if (!string.IsNullOrWhiteSpace(_ShowPropertiesContaining) && p.Name.ToLower().Contains(_ShowPropertiesContaining)) filter |= true;

                    if (!filter) continue;
                    
                    props.Add(new Tuple<string, string, string>(p.Name, p.UnevaluatedValue, p.EvaluatedValue));
                }

                return props;
            }
        }

        public IEnumerable<KeyValuePair<string, string>> EvaluatedItems
        {
            get
            {
                return _Project
                    .AllEvaluatedItems
                    .Select(item => new KeyValuePair<string, string>(item.ItemType, item.EvaluatedInclude))
                    .ToList();
            }
        }

        #endregion
    }    
}
