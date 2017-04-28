using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisualSolutionGenerator
{
    public class CommandLineConstants
    {
        #region lifecycle

        public CommandLineConstants()
        {
            var args = System.Environment.GetCommandLineArgs();

            if (args.Length > 0) // remove calling executable from arguments
            {
                if (args[0].ToLower().EndsWith(".exe")) args = args.Skip(1).ToArray();
            }            

            Values = args;      
        }

        public CommandLineConstants(string[] args) { Values = args; }        

        #endregion

        #region data

        private readonly HashSet<string> _Args = new HashSet<string>();

        #endregion

        #region properties

        public IEnumerable<string> Values
        {
            get { return _Args; }
            set
            {
                _Args.Clear();
                if (value == null) return;
                _Args.UnionWith(value.Where(item => !string.IsNullOrEmpty(item)));
            }
        }

        public int Count { get { return _Args.Count; } }

        #endregion

        #region API

        private static string _ToAbsolute(string path)
        {
            if (System.IO.Path.IsPathRooted(path)) return path;
            return System.IO.Path.Combine(System.Environment.CurrentDirectory, path);
        }

        public bool Contains(string arg) { return _Args.Any( item => item.StartsWith(arg) ); }

        public bool ContainsCommand(string arg)
        {
            if (arg.EndsWith(":")) return _Args.Any(item => item.StartsWith(arg));

            return _Args.Any(item => item == arg);
        }

        public string Get(string key, string defval)
        {
            var val = _Args.Where(item => item.StartsWith(key)).Select(item => item.Substring(key.Length)).FirstOrDefault();

            return string.IsNullOrEmpty(val) ? defval : val;
        }

        public string GetAbsolutePath(string key, string defval = null)
        {
            var path = Get(key, defval); if (path == null) return null;
            return _ToAbsolute(path);
        }

        public string GetInputFile(string key, string defval = null)
        {
            var path = Get(key, defval); if (path == null) return null;
            path = _ToAbsolute(path);

            var finfo = new System.IO.FileInfo(path);

            return finfo.Exists ? finfo.FullName : null;            
        }

        public string GetOutputFile(string key, string defval = null)
        {
            var path = Get(key, defval); if (path == null) return null;
            path = _ToAbsolute(path);

            return path;
        }

        public string GetInputDirectory(string key, string defval = null)
        {
            var path = Get(key, defval); if (path == null) return null;
            path = _ToAbsolute(path);

            var finfo = new System.IO.DirectoryInfo(path);

            return finfo.Exists ? finfo.FullName : null;
        }

        public string GetWithAuto(string key, string defval)
        {
            var val = _Args.Where(item => item.StartsWith(key)).Select(item => item.Substring(key.Length)).FirstOrDefault();

            return string.IsNullOrEmpty(val) || val == "Auto" ? defval : val;
        }


        public bool GetBool(string key, bool defval)
        {
            var v = GetWithAuto(key, defval ? "true" : "false").ToLowerInvariant();

            return v == "true";
        }

                

        public void Clear(string arg)
        {
            var idx = arg.IndexOf(':');

            arg = idx < 0 ? arg : arg.Substring(0, idx + 1);

            var args = _Args.Where(item => item.StartsWith(arg));

            foreach (var a in args.ToArray()) _Args.Remove(a);            
        }

        public void Set(string arg)
        {
            if (string.IsNullOrEmpty(arg)) return;

            Clear(arg);

            _Args.Add(arg);            
        }

        public override string ToString() { return string.Join(" ", _Args.ToArray()); }

        #endregion
    }
}
