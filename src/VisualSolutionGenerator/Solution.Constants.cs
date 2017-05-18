﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VisualSolutionGenerator
{
    static class SolutionConstants
    {
        #region data       

        // From Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\8.0\Projects\
        public static readonly Guid FolderGUID = new Guid("2150E333-8FDC-42A3-9474-1A3956D46DE8");

        private static readonly IDictionary<string, Guid> _ProjGUIDs;

        static SolutionConstants()
        {
            _ProjGUIDs = new Dictionary<string, Guid>();
            _ProjGUIDs["ccproj"] = new Guid("cc5fd16d-436d-48ad-a40c-5a424c6e3e79");
            _ProjGUIDs["csproj"] = new Guid("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");
            _ProjGUIDs["dbproj"] = new Guid("006AC3C3-E446-4C8A-B35F-CBA0E2238D60");
            _ProjGUIDs["vbproj"] = new Guid("F184B08F-C81C-45f6-A57F-5ABD9991F28F");
            _ProjGUIDs["shproj"] = new Guid("D954291E-2A0B-460D-934E-DC6B0785DB48");
        }

        #endregion

        #region API

        public static Guid GetProjectGuidFromExt(string extension)
        {
            extension = extension.ToLower().Trim().TrimStart('.');

            Guid projTypeGuid;
            return _ProjGUIDs.TryGetValue(extension, out projTypeGuid) ? projTypeGuid : Guid.Empty;
        }

        

        #endregion
    }
}