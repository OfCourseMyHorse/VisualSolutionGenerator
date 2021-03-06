﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace VisualSolutionGenerator.Solutions
{
    public static class SolutionBuilder2017
    {
        #region API

        public static void WriteSolutionFile(string solutionFile, FileBaseInfo.Collection context)
        {
            if (context == null) return;

            var validProjectFiles = context.ProjectFiles.Where(prj => prj.Solution != null).ToArray();
            if (validProjectFiles.Length == 0) return;

            var root = _VirtualFolder.CreateRoot(validProjectFiles);

            System.Diagnostics.Debug.Assert(solutionFile != null);

            using (var writer = new StringWriter())
            {
                writer.WriteLine();
                writer.WriteLine("Microsoft Visual Studio Solution File, Format Version 12.00");
                writer.WriteLine("# Visual Studio 2015");
                writer.WriteLine("VisualStudioVersion = 15.0.26403.7");
                writer.WriteLine("MinimumVisualStudioVersion = 10.0.40219.1");
                writer.WriteLine();
                writer.WriteLine();

                // folder alias           

                foreach (var f in root.AllFolders.Where(item => item.FolderId != Guid.Empty)) { _WriteVirtualFolderEntry(writer, f, Path.GetDirectoryName(solutionFile)); }

                // Projects

                foreach (var p in validProjectFiles) { _WriteProjectEntry(writer, p, Path.GetDirectoryName(solutionFile)); }

                // Project and folder relations

                writer.WriteLine("Global");

                _WriteAnhkSVNSection(writer);

                _WriteNestedProjects(validProjectFiles, root, writer);

                writer.WriteLine("EndGlobal");

                writer.Flush();

                System.IO.File.WriteAllText(solutionFile, writer.ToString());
            }

        }


        private static void _WriteVirtualFolderEntry(TextWriter writer, _VirtualFolder virtualFolder, string rootFolder)
        {
            string format = "Project('{0}') = '{1}', '{2}', '{3}'".Replace('\'', '"');
            writer.WriteLine(format, Constants.FolderGUID.ToString("B"), virtualFolder.Name, virtualFolder.Name, virtualFolder.FolderId.ToString("B"));


            if (false)
            {
                // this is for solution wide documents, text and referenced assemblies

                writer.WriteLine("\tProjectSection(SolutionItems) = preProject");
                foreach (var path in new string[0])
                {
                    writer.WriteLine("\t\t{0} = {1}", path, path);
                }
                writer.WriteLine("\tEndProjectSection");
            }
            
            writer.WriteLine("EndProject");
        }

        private static void _WriteProjectEntry(TextWriter writer, FileProjectInfo.View prj, string rootFolder)
        {
            var ppath = Utils.GetRelativePath(rootFolder, prj.FilePath);

            var format = "Project('{0}') = '{1}', '{2}', '{3}'".Replace('\'', '"');

            var projTypeGuid = Constants.GetProjectGuidFromExt(Path.GetExtension(prj.FilePath));

            writer.WriteLine(format, projTypeGuid.ToString("B"), prj.FileName, ppath, prj.ProjectId.ToString("B"));            
            
            writer.WriteLine("EndProject");
        }

        private static void _WriteNestedProjects(IEnumerable<FileProjectInfo.View> projects, _VirtualFolder root, StringWriter writer)
        {
            writer.WriteLine("\tGlobalSection(NestedProjects) = preSolution");            

            string format = "\t\t{0} = {1}";

            // Folder relations
            foreach (var folder in root.AllFolders)
            {
                if (folder.FolderId == Guid.Empty || folder.ParentId == Guid.Empty) continue;

                writer.WriteLine(format, folder.FolderId.ToString("B"), folder.ParentId.ToString("B"));
            }

            // Project relations
            foreach (var prj in projects.Where(p => p.ProjectId != Guid.Empty))
            {
                var folderGuid = root.GetVirtualFolderId(prj); if (folderGuid == Guid.Empty) continue;

                writer.WriteLine(format, prj.ProjectId.ToString("B"), folderGuid.ToString("B"));
            }

            writer.WriteLine("\tEndGlobalSection");
        }

        private static void _WriteAnhkSVNSection(TextWriter writer)
        {
            writer.WriteLine("\tGlobalSection(SubversionScc) = preSolution");            

            string format = "\t\t{0} = {1}";

            writer.WriteLine(format, "Svn-Managed", "True");
            writer.WriteLine(format, "Manager", "AnkhSVN - Subversion Support for Visual Studio");

            writer.WriteLine("\tEndGlobalSection");
        }

        #endregion
    }    
}
