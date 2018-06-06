using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace VisualSolutionGenerator
{
    
    public partial class App : Application
    {
        #region MAIN

        [STAThread]
        public static void Main()
        {
            _ProcessSolutionGenFile();

            if (CommandLineParser.Default.IsSilent) { _ProcessSilent(); return; }

            var application = new App();            
            application.InitializeComponent();
            application.Run();
        }

        private static void _ProcessSolutionGenFile()
        {
            var slnGenFilePath = CommandLineParser.Default.SlnGenFilePath;

            if (string.IsNullOrWhiteSpace(slnGenFilePath)) return;
            if (!System.IO.File.Exists(slnGenFilePath)) return;
            
            System.Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(slnGenFilePath);

            var slngen = System.IO.File.ReadAllLines(slnGenFilePath);
            slngen = slngen.Select(l => l.Replace("\"", "")).ToArray();

            CommandLineParser.Default.UpdateParameters(slngen);            
        }

        private static void _ProcessSilent()
        {
            var engine = new EngineContext();
            engine.ProcessCommandLine(CommandLineParser.Default);

            engine.GenerateSolution();

            if (!string.IsNullOrWhiteSpace(CommandLineParser.Default.RunAfter))
            {
                System.Diagnostics.Process.Start(CommandLineParser.Default.RunAfter);
            }
        }       
        

        #endregion
    }


    class AppView : BindableBase
    {
        #region lifecycle

        public AppView()
        {
            IncludeDirectoryTreeCmd = new RelayCommand(_SelectDirectoryTree);
            IncludeSingleProjectCmd = new RelayCommand(_SelectSingleProjectPath);
            BrowseSolutionPathCmd = new RelayCommand(_SetTargetSolutionPath);

            GenerateCmd = new RelayCommand(_Engine.GenerateSolution);

            SaveDgmlCmd = new RelayCommand(_SaveDgmlFile);

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
        public ICommand BrowseSolutionPathCmd { get; private set; }

        public ICommand GenerateCmd { get; private set; }

        public ICommand SaveDgmlCmd { get; private set; }

        #endregion

        #region API

        private void _SelectDirectoryTree()
        {
            using (var dlg = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog("Select Source Directory"))
            {
                dlg.IsFolderPicker = true;
                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() != Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok) return;

                _Engine.AddDirectoryTree(dlg.FileName, null, null);
            }
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

            _Engine.GenerateSolution();
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

            _Engine.GenerateDgml(dlg.FileName);
        }

        #endregion
    }
}
