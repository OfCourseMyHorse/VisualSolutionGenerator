using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;

namespace VisualSolutionGenerator
{
    sealed class EngineContext : BindableBase
    {
        #region data

        /// <summary>
        /// Collection of project files
        /// </summary>
        private FileBaseInfo.Collection _Projects;

        /// <summary>
        /// Path where we save the solution
        /// </summary>
        private string _SolutionSavePath;

        #endregion

        #region properties        

        public IEnumerable<FileProjectInfo> Projects => _Projects == null ? Enumerable.Empty<FileProjectInfo>() : _Projects.ProjectFiles.ToArray();

        public IEnumerable<FileErrorInfo> FailedFiles => _Projects == null ? Enumerable.Empty<FileErrorInfo>() : _Projects.FailedFiles.ToArray();

        public string SolutionPath
        {
            get => _SolutionSavePath != null ? _SolutionSavePath : SolutionConstants.GetSemanticVersion();
            set { _SolutionSavePath = value; RaiseChanged(nameof(SolutionPath)); }
        }
                
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

        public void SaveSolution()
        {
           SolutionBuilder2017.WriteSolutionFile(SolutionPath, _Projects);
        }

        public void SaveDgml(string filePath)
        {
            var dgml = _Projects.ToDGML();

            using (var writer = new System.IO.StreamWriter(filePath))
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(OpenSoftware.DgmlTools.Model.DirectedGraph));
                serializer.Serialize(writer, dgml);
            }
        }

        public void SaveProjectsMetrics(string directoryPath, Func<int, string, bool> monitor)
        {
            System.IO.Directory.CreateDirectory(directoryPath);

            // TODO: force restore package, then find exe

            var exePath = @"C:\Users\vpena\.nuget\packages\microsoft.codeanalysis.metrics\2.9.3\Metrics\Metrics.exe";

            int part = 0;
            int total = _Projects.ProjectFiles.Count();

            if (total == 0) return;

            total += 1;

            var metricFiles = new List<string>();

            foreach(var prj in _Projects.ProjectFiles)
            {
                if (monitor((part * 100 / total), prj.FilePath)) break;

                var prjPath = prj.FilePath;
                var mtcPath = System.IO.Path.Combine(directoryPath, System.IO.Path.GetFileNameWithoutExtension( System.IO.Path.GetFileName(prjPath)) + ".xml");

                metricFiles.Add(mtcPath);

                var psi = new System.Diagnostics.ProcessStartInfo(exePath);
                psi.Arguments = $"/p:\"{prjPath}\" /out:\"{mtcPath}\"";
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;

                System.Diagnostics.Process
                    .Start(psi)
                    .WaitForExit();

                ++part;
            }

            if (monitor(((total -1) / total), "table")) return;


            var sb = new StringBuilder();

            foreach(var f in metricFiles)
            {
                if (!System.IO.File.Exists(f)) continue;
                var xml = System.Xml.Linq.XElement.Load(f);

                foreach(var target in xml.Descendants("Targets"))
                {
                    var assembly = target.Descendants("Assembly").First();
                    var assemblyName = assembly.Attribute("Name").Value.Replace(" ","_");

                    sb.Append($"{assemblyName}, ");

                    var metrics = assembly.Descendants("Metrics").First();

                    foreach(var metric in metrics.Descendants("Metric"))
                    {
                        var k = metric.Attribute("Name").Value;
                        var v = metric.Attribute("Value").Value;

                        sb.Append($"{v}, ");
                    }

                    sb.AppendLine();
                }
            }

            System.IO.File.WriteAllText(System.IO.Path.Combine(directoryPath, "metrics.csv"), sb.ToString());
        }

        #endregion
    }
}
