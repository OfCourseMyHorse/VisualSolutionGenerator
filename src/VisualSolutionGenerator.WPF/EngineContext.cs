﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

          
namespace VisualSolutionGenerator
{
    sealed class EngineContext : BindableBase
    {
        #region data

        private FileBaseInfo.Collection _Projects;

        private string _SolutionPath;

        #endregion

        #region properties        

        public IEnumerable<ProjectInfo> Projects => _Projects == null ? Enumerable.Empty<ProjectInfo>() : _Projects.ProjectFiles.ToArray();

        public IEnumerable<FileErrorInfo> FailedFiles => _Projects == null ? Enumerable.Empty<FileErrorInfo>() : _Projects.FailedFiles.ToArray();

        public string SolutionPath { get { return _SolutionPath; } set { _SolutionPath = value; RaiseChanged(nameof(SolutionPath)); } }
                
        #endregion

        #region API

        public void ProcessCommandLine(CommandLineParser args)
        {
            if (args.IsProjectMode) AddSingleProject(args.RootProjectPath);
            if (args.IsDirectoryMode) AddDirectoryTree(args.RootDirectoryPath, args.ExcludeProjects,args.ExcludeDirectories);

            foreach(var extraProj in args.IncludeProjects) AddSingleProject(extraProj);

            // do this at the end
            if (!string.IsNullOrWhiteSpace(args.TargetSolutionPath)) SolutionPath = args.TargetSolutionPath;
        }

        public void AddSingleProject(string filePath)
        {
            var finfo = new System.IO.FileInfo(filePath);

            if (_Projects == null) _Projects = new FileBaseInfo.Collection(finfo.Directory);

            var prj = _Projects.UseProject(finfo);

            if (prj != null)
            {
                prj.Solution.VirtualFolderPath = null;

                if (string.IsNullOrWhiteSpace(SolutionPath))
                {
                    var dir = System.IO.Path.GetDirectoryName(prj.FileDirectory);
                    dir = System.IO.Path.Combine(dir, prj.FileName + ".tmp");

                    SolutionPath = System.IO.Path.ChangeExtension(dir, ".Generated.sln");
                }
            }

            RaiseChanged(nameof(Projects), nameof(FailedFiles));
        }

        public void AddDirectoryTree(string directoryPath, IEnumerable<string> excludePrjs, IEnumerable<string> excludeDirs)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(SolutionPath))
                {
                    var dir = System.IO.Path.Combine(directoryPath, "GlobalView.tmp");
                    SolutionPath = System.IO.Path.ChangeExtension(dir, ".Generated.sln");
                }

                if (_Projects == null) _Projects = new FileBaseInfo.Collection(new System.IO.DirectoryInfo(System.IO.Path.GetDirectoryName(SolutionPath)));


                _Projects.ProbeFolder(directoryPath, excludePrjs, excludeDirs, true);

                RaiseChanged(nameof(Projects), nameof(FailedFiles));
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error");
            }
        }

        public void GenerateSolution()
        {
           SolutionBuilder2017.WriteSolutionFile(SolutionPath, _Projects);
        }

        #endregion
    }
}
