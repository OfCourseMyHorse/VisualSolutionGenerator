using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace VisualSolutionGenerator
{
    class AppView : BindableBase
    {
        #region lifecycle

        public AppView()
        {
            IncludeDirectoryTreeCmd = new RelayCommand(_SelectDirectoryTree);
            IncludeSingleProjectCmd = new RelayCommand(_SelectSingleProjectPath);

            SaveSolutionCmd = new RelayCommand(_Engine.SaveSolution);
            SaveSolutionAsCmd = new RelayCommand(_SetTargetSolutionPath);
            SaveDgmlCmd = new RelayCommand(_SaveDgmlFile);
            SaveMetricsCmd = new RelayCommand(_SaveMetrics);

            RegisterFileAssociationCmd = new RelayCommand(_RegisterExtensions);

            _Engine.ProcessCommandLine(CommandLineParser.Default);
        }

        #endregion

        #region data

        private readonly EngineContext _Engine = new EngineContext();

        #endregion

        #region properties

        public EngineContext Engine { get { return _Engine; } }

        public ICommand IncludeSingleProjectCmd { get; private set; }
        public ICommand IncludeDirectoryTreeCmd { get; private set; }

        public ICommand SaveSolutionCmd { get; private set; }
        public ICommand SaveSolutionAsCmd { get; private set; }
        public ICommand SaveDgmlCmd { get; private set; }
        public ICommand SaveMetricsCmd { get; private set; }

        public ICommand RegisterFileAssociationCmd { get; private set; }

        #endregion

        #region API

        private void _SelectDirectoryTree()
        {
            

            var path = _OpenFolderBrowserDialog("Select root directory to begin scanning for projects.");

            if (path != null) _Engine.AddDirectoryTree(path, null, null);
        }

        private void _SelectSingleProjectPath()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                RestoreDirectory = true,
                Filter = "Visual Studio Project|*.csproj;*.shproj"
            };

            var defpath = _Engine.SolutionPath;
            if (!string.IsNullOrWhiteSpace(defpath))
            {
                dlg.FileName = defpath;
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(defpath);
            }

            if (!dlg.ShowDialog(System.Windows.Application.Current.MainWindow).Value) return;

            _Engine.AddSingleProject(dlg.FileName);
        }

        private void _SetTargetSolutionPath()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                RestoreDirectory = true,
                Filter = "Visual Studio Solution|*.sln",

                FileName = _Engine.SolutionPath
            };

            if (!dlg.ShowDialog(System.Windows.Application.Current.MainWindow).Value) return;

            _Engine.SolutionPath = dlg.FileName;

            _Engine.SaveSolution();
        }

        private void _SaveDgmlFile()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                RestoreDirectory = true,
                Filter = "Directed Graph|*.dgml",

                FileName = System.IO.Path.ChangeExtension(_Engine.SolutionPath, ".dgml")
            };

            if (!dlg.ShowDialog(System.Windows.Application.Current.MainWindow).Value) return;

            _Engine.SaveDgml(dlg.FileName);
        }

        private void _SaveMetrics()
        {
            string dstDir = _OpenFolderBrowserDialog("Select Metrics target Directory");
            if (dstDir == null) return;
            
            using (var dlg = new Microsoft.WindowsAPICodePack.Dialogs.TaskDialog())
            {
                dlg.ProgressBar = new Microsoft.WindowsAPICodePack.Dialogs.TaskDialogProgressBar();
                dlg.Cancelable = true;

                dlg.ProgressBar.Minimum = 0;
                dlg.ProgressBar.Maximum = 100;

                bool monitor(int percent, string msg)
                {
                    dlg.ProgressBar.Value = percent;
                    return false;
                }

                void process()
                {
                    _Engine.SaveProjectsMetrics(dstDir, monitor);
                    dlg.Close();
                }

                dlg.Opened += (s, e) =>
                {
                    System.Threading.Tasks.Task.Factory.StartNew(process);
                };                

                dlg.Show();                
            }
        }

        private void _RegisterExtensions()
        {
            // https://stackoverflow.com/questions/29847034/how-to-show-set-program-associations-window-in-windows-8-8-1

            var appRegSrc = new Orc.FileAssociation.ApplicationRegistrationService() as Orc.FileAssociation.IApplicationRegistrationService;
            var extRegSrc = new Orc.FileAssociation.FileAssociationService() as Orc.FileAssociation.IFileAssociationService;

            // register application
            var assembly = Catel.Reflection.AssemblyHelper.GetEntryAssembly();
            var applicationInfo = new Orc.FileAssociation.ApplicationInfo(assembly);

            appRegSrc.UnregisterApplication(applicationInfo);

            applicationInfo.SupportedExtensions.Clear();
            applicationInfo.SupportedExtensions.Add("slngen");

            if (!appRegSrc.IsApplicationRegistered(applicationInfo)) appRegSrc.RegisterApplication(applicationInfo);            

            // register extension
            extRegSrc.AssociateFilesWithApplication(applicationInfo.Name);
        }


        private string _OpenFolderBrowserDialog(string description)
        {
            // https://github.com/dotnet/wpf/issues/438#issuecomment-479944544

            using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                // dlg.UseDescriptionForTitle = !string.IsNullOrWhiteSpace(description);
                dlg.Description = description; // "Select root directory to begin scanning for projects."
                if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return null;

                return dlg.SelectedPath;
            }
        }

        #endregion
    }

    
}
