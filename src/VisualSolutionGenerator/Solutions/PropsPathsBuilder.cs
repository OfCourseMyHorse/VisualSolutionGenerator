using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualSolutionGenerator.Solutions
{
    public class PropsPathsBuilder
    {
        public static void WritePropsFile(string propsFile, FileBaseInfo.Collection context)
        {
            if (context == null) return;

            var validProjectFiles = context.ProjectFiles.Where(prj => prj.Solution != null).ToArray();
            if (validProjectFiles.Length == 0) return;            

            using (var writer = new StringWriter())
            {
                writer.WriteLine();
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf - 8\"?>");
                writer.WriteLine("<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\" DefaultTargets=\"Build\" ToolsVersion=\"4.0\">");
                
                writer.WriteLine();

                // Projects

                writer.WriteLine("  <PropertyGroup>");

                foreach (var p in validProjectFiles)
                {
                    _WriteProjectEntry(writer, p, Path.GetDirectoryName(propsFile));
                }

                writer.WriteLine("  </PropertyGroup>");
                writer.WriteLine();
                writer.WriteLine("</Project>");

                writer.Flush();

                System.IO.File.WriteAllText(propsFile, writer.ToString());
            }
        }

        private static void _WriteProjectEntry(TextWriter writer, FileProjectInfo.View prj, string rootFolder)
        {
            var fname = prj.FileName.Replace(".",string.Empty);

            fname += "_csproj";

            fname = fname.ToUpper();

            var ppath = Utils.GetRelativePath(rootFolder, prj.FilePath);

            //if (ppath.Any(c => char.IsWhiteSpace(c))) ppath = $"\"{ppath}\"";

            writer.WriteLine($"    <{fname}>$(MSBuildThisFileDirectory){ppath}</{fname}>");
        }
    }
}
