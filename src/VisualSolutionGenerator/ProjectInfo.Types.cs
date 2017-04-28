using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualSolutionGenerator
{
    public static class ProjectInfoTypes
    {
        #region programming languages

        // http://www.codeproject.com/Reference/720512/List-of-Visual-Studio-Project-Type-GUIDs

        public static readonly Guid LANG_CPLUSPLUS = Guid.Parse("{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}");
        public static readonly Guid LANG_CSHARP = Guid.Parse("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}");
        public static readonly Guid LANG_FSHARP = Guid.Parse("{F2A71F9B-5D33-465A-A702-920D77279786}");
        public static readonly Guid LANG_JSHARP = Guid.Parse("{E6FDF86B-F3D1-11D4-8576-0002A516ECE8}");
        // Missing: python, Golang

        public static readonly Guid WIN_CSHARP = LANG_CSHARP;
        public static readonly Guid WIN_VBNET = Guid.Parse("{F184B08F-C81C-45F6-A57F-5ABD9991F28F}");
        public static readonly Guid WIN_VCPLUSPLUS = LANG_CPLUSPLUS;

        public static bool IsProgrammingLanguage(this Guid id)
        {
            if (id == LANG_CPLUSPLUS) return true;
            if (id == LANG_CSHARP) return true;
            if (id == LANG_FSHARP) return true;
            if (id == LANG_JSHARP) return true;
            if (id == WIN_VBNET) return true;
            return false;
        }

        #endregion

        #region technologies

        public static readonly Guid WCF = Guid.Parse("{3D9AD99F-2412-4246-B90B-4EAA41C64699}");
        public static readonly Guid WPF = Guid.Parse("{60DC8134-EBA5-43B8-BCC9-BB4BC16C2548}");

        public static readonly Guid XNA_WIN = Guid.Parse("{6D335F3A-9D43-41b4-9D22-F6F17C4BE596}");
        public static readonly Guid XNA_CONTENT = Guid.Parse("{96E2B04D-8817-42c6-938A-82C39BA4D311}");

        public static readonly Guid PORTABLE_CLASS_LIBRARY = Guid.Parse("{786C830F-07A1-408B-BD7F-6EE04809D6DB}");

        public static readonly Guid ASPNET_MVC5 = Guid.Parse("{349C5851-65DF-11DA-9384-00065B846F21}");

        #endregion

        #region Platform UWP

        public static readonly Guid WIN_STORE = Guid.Parse("{BC8A1FFA-BEE3-4634-8014-F334798102B3}");           // maybe Win8.1 Store ?
        public static readonly Guid UWP_CLASS_LIBRARY = Guid.Parse("{A5A43C5B-DE2A-4C0C-9213-0A381AF9435A}"); // seems it is also used by EXE files
        public static readonly Guid WINDOWS_PHONE_8 = Guid.Parse("{76F1466A-8B6D-4E39-A767-685A06062A39}");

        public static bool IsPlatformUniversal(this Guid id)
        {
            if (id == WIN_STORE) return true;
            if (id == UWP_CLASS_LIBRARY) return true;
            return false;
        }

        #endregion

        #region Platform Android

        public static readonly Guid XAMARIN_ANDROID = Guid.Parse("{EFBA0AD7-5A72-4C68-AF49-83D382785DCF}");

        public static bool IsPlatformAndroid(this Guid id)
        {
            if (id == XAMARIN_ANDROID) return true;            
            return false;
        }

        #endregion

        #region Platform iOS

        // https://developer.xamarin.com/guides/cross-platform/macios/unified/updating-ios-apps/
        public static readonly Guid IOS_OBSOLETE = Guid.Parse("{6BC8ED88-2882-458C-8E55-DFD12B67127B}");
        public static readonly Guid IOS = Guid.Parse("{FEACFBD2-3405-455C-9665-78FE426C6842}");

        public static bool IsPlatformIOS(this Guid id)
        {
            if (id == IOS) return true;
            if (id == IOS_OBSOLETE) return true;
            return false;
        }

        #endregion

        #region API

        public static bool IsTargetPlatform(this Guid id)
        {
            if (IsPlatformUniversal(id)) return true;
            if (IsPlatformAndroid(id)) return true;
            if (IsPlatformIOS(id)) return true;
            return false;
        }

        private sealed class _Comparer : IComparer<Guid>
        {
            private _Comparer() { }

            private static readonly _Comparer _Default = new _Comparer();

            public static _Comparer Default => _Default;

            public int Compare(Guid x, Guid y)
            {
                if (x.IsProgrammingLanguage() && !y.IsProgrammingLanguage()) return -1;
                if (!x.IsProgrammingLanguage() && y.IsProgrammingLanguage()) return 1;

                if (x.IsTargetPlatform() && !y.IsTargetPlatform()) return -1;
                if (!x.IsTargetPlatform() && y.IsTargetPlatform()) return 1;

                return x.CompareTo(y);
            }
        }

        public static IComparer<Guid> Comparer => _Comparer.Default;

        private static Dictionary<Guid, string> _Dictionary = null;

        public static IReadOnlyDictionary<Guid,string> GetDictionary()
        {
            if (_Dictionary != null) return _Dictionary;

            var dict = new Dictionary<Guid, string>();

            dict.Add(LANG_CPLUSPLUS, "C++");
            dict.Add(LANG_CSHARP, "C#");
            dict.Add(LANG_FSHARP, "F#");
            dict.Add(LANG_JSHARP, "J#");

            // dict.Add(WIN_CSHARP, "Windows (C#)");
            dict.Add(WIN_VBNET, "Windows (VB.NET)");
            // dict.Add(WIN_VCPLUSPLUS, "Windows (Visual C++)");
            dict.Add(WIN_STORE, "Windows Store (Metro) Apps & Components");
            
            dict.Add(WCF, "Windows Communication Foundation (WCF)");
            // dict.Add(MONO_ANDROID, "Mono for Android");

            dict.Add(PORTABLE_CLASS_LIBRARY, "Portable Class Library");

            dict.Add(UWP_CLASS_LIBRARY, "Universal Windows Class Library");

            dict.Add(WPF, "Windows Presentation Foundation (WPF)");

            dict.Add(WINDOWS_PHONE_8, "Windows Phone 8/8.1 Blank/Hub/Webview App");

            dict.Add(XAMARIN_ANDROID, "Xamarin.Android");            

            dict.Add(XNA_WIN, "XNA (Windows)");
            dict.Add(XNA_CONTENT, "XNA (Content)");

            dict.Add(ASPNET_MVC5, "ASP.NET MVC 5");

            dict.Add(IOS_OBSOLETE, "Xamarin.iOS(Obsolete)");
            dict.Add(IOS, "Xamarin.iOS");            

            _Dictionary = dict;
            return dict;
        }

        public static string GetProjectTypeDescription(this Guid projectType)
        {
            var dict = GetDictionary();

            return dict.ContainsKey(projectType) ? dict[projectType] : projectType.ToString();
        }

        #endregion
    }
}
