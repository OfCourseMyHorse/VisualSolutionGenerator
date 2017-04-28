using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VisualSolutionGenerator
{
    static class Utils
    {
        #region Utility methods

        public static string GetAssemblyNameFromFullyQualifiedName(string fullyQualifiedName)
        {
            string assemblyName = fullyQualifiedName;
            if (assemblyName.IndexOf(",") != -1)
            {
                assemblyName = assemblyName.Remove(assemblyName.IndexOf(","));
            }
            return assemblyName;
        }

        public static string GetRelativePath(string fromPath, string toPath)
        {
            var fromDirectories = fromPath.Split(Path.DirectorySeparatorChar);
            var toDirectories = toPath.Split(Path.DirectorySeparatorChar);

            // Get the shortest of the two paths
            int length = fromDirectories.Length < toDirectories.Length
                             ? fromDirectories.Length
                             : toDirectories.Length;

            int lastCommonRoot = -1;
            int index;

            // Find common root
            for (index = 0; index < length; index++)
            {
                if (fromDirectories[index].Equals(toDirectories[index], StringComparison.OrdinalIgnoreCase))
                {
                    lastCommonRoot = index;
                }
                else
                {
                    break;
                }
            }

            // If we didn't find a common prefix then abandon
            if (lastCommonRoot == -1)
            {
                return null;
            }

            // Add the required number of "..\" to move up to common root level
            var relativePath = new StringBuilder();
            for (index = lastCommonRoot + 1; index < fromDirectories.Length; index++)
            {
                relativePath.Append(".." + Path.DirectorySeparatorChar);
            }

            // Add on the folders to reach the destination
            for (index = lastCommonRoot + 1; index < toDirectories.Length - 1; index++)
            {
                relativePath.Append(toDirectories[index] + Path.DirectorySeparatorChar);
            }
            relativePath.Append(toDirectories[toDirectories.Length - 1]);

            return relativePath.ToString();
        }

        public static bool IsInIgnoreList(string folder, IEnumerable<string> ignoreList)
        {
            if (folder == null) return false;
            if (ignoreList == null) return false;

            foreach (string ignore in ignoreList)
            {
                if (folder.StartsWith(ignore, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
       
}
