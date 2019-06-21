using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace VisualSolutionGenerator
{    
    public partial class App : Application
    {
        #region MAIN

        [STAThread]
        public static void Main()
        {
            Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();            

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

            engine.SaveSolution();

            if (!string.IsNullOrWhiteSpace(CommandLineParser.Default.RunAfter))
            {
                System.Diagnostics.Process.Start(CommandLineParser.Default.RunAfter);
            }
        }       
        

        #endregion
    }    
}
