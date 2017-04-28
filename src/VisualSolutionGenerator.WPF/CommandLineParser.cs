using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisualSolutionGenerator
{

    class CommandLineParser
    {
        #region lifecycle

        private CommandLineParser() { }

        public static readonly CommandLineParser Default = new CommandLineParser();

        #endregion

        #region data

        private CommandLineConstants _Args = new CommandLineConstants();

        #endregion

        #region properties

        public bool IsProjectMode { get { return !string.IsNullOrWhiteSpace(RootProjectPath); } }
        public bool IsDirectoryMode { get { return !string.IsNullOrWhiteSpace(RootDirectoryPath); } }

        public bool IsOpenVerb { get { return !string.IsNullOrEmpty(_GetOpenVerbParam()); } }

        public bool IsSilent { get { return _Args.Contains("-Silent") || IsOpenVerb;  } }

        public string SlnGenFilePath { get { return _Args.GetInputFile("-SlnGen:", _GetVerbOpenIf(".slngen")); } }

        public string RootProjectPath { get { return _Args.GetInputFile("-RootPrj:", _GetVerbOpenIf(".csproj")); } }

        

        // note: to force a self directory use:  -RootDir:".\"
        public string RootDirectoryPath { get { return _Args.GetInputDirectory("-RootDir:"); } }

        public string TargetSolutionPath { get { return _Args.GetOutputFile("-TargetSln:"); } }

        public string RunAfter { get { return _Args.GetAbsolutePath("-RunAfter:"); } }

        // -IncludePrjs:"alpha.csproj, beta.csproj"
        public IEnumerable<string> IncludeProjects
        {
            get
            {
                var prjs = _Args.Get("-IncludePrjs:", null);

                if (string.IsNullOrWhiteSpace(prjs)) return Enumerable.Empty<string>();

                return prjs.Split(',').Select(n => n.Trim());
            }
        }

        // -ExcludePrjs:"alpha.csproj, beta.csproj"
        public IEnumerable<string> ExcludeProjects
        {
            get
            {
                var prjs = _Args.Get("-ExcludePrjs:", null);

                if (string.IsNullOrWhiteSpace(prjs)) return Enumerable.Empty<string>();

                return prjs.Split(',').Select(n=> n.Trim());
            }
        }

        

        // -ExcludeDirs:"alpha, beta"
        public IEnumerable<string> ExcludeDirectories
        {
            get
            {
                if (!IsDirectoryMode) return Enumerable.Empty<string>();

                var prjs = _Args.Get("-ExcludeDirs:", null);

                if (string.IsNullOrWhiteSpace(prjs)) return Enumerable.Empty<string>();

                return prjs.Split(',')
                    .Select(n => n.Trim())
                    .Select(p => System.IO.Path.Combine(RootDirectoryPath, p));
            }
        }

        #endregion

        #region API

        public void UpdateParameters(params string[] args) { _Args = new CommandLineConstants(args); }        

        private string _GetOpenVerbParam()
        {
            if (_Args.Count != 1) return null;            

            var openVerbPath = _Args.Values.FirstOrDefault();

            return System.IO.File.Exists(openVerbPath) ? openVerbPath : null;
        }

        public string VerbOpenParam { get { return _GetOpenVerbParam(); } }        

        private string _GetVerbOpenIf(string ext)
        {
            if (string.IsNullOrWhiteSpace(VerbOpenParam)) return null;
            if (!VerbOpenParam.EndsWith(ext)) return null;
            return VerbOpenParam;
        }

        #endregion
    }

    
}
