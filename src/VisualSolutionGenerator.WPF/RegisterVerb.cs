using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisualSolutionGenerator
{

    using REGKEY = Microsoft.Win32.RegistryKey;

    // shell extension handlers - context menu handlers
    // https://msdn.microsoft.com/en-us/library/windows/desktop/cc144110%28v=vs.85%29.aspx

    // http://www.codeproject.com/Articles/17023/System-File-Association

    // http://stackoverflow.com/questions/3102562/how-does-one-add-a-secondary-verb-to-a-file-type-in-windows-shell
    // http://stackoverflow.com/questions/1387769/create-registry-entry-to-associate-file-extension-with-application-in-c


    // http://www.codeproject.com/Articles/441/The-Complete-Idiot-s-Guide-to-Writing-Shell-Extens#registering

    // https://msdn.microsoft.com/en-us/library/windows/desktop/cc144104%28v=vs.85%29.aspx

    // http://stackoverflow.com/questions/69761/how-to-associate-a-file-extension-to-the-current-executable-in-c-sharp

    public class RegisterExtensionVerb
    {
        #region PInvoke

        // http://www.pinvoke.net/default.aspx/shell32.shchangenotify

        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        private static extern void SHChangeNotify(HChangeNotifyEventID wEventId,HChangeNotifyFlags uFlags,IntPtr dwItem1,IntPtr dwItem2);

        /// <summary>
        /// Describes the event that has occurred.
        /// Typically, only one event is specified at a time.
        /// If more than one event is specified, the values contained
        /// in the <i>dwItem1</i> and <i>dwItem2</i>
        /// parameters must be the same, respectively, for all specified events.
        /// This parameter can be one or more of the following values.
        /// </summary>
        /// <remarks>
        /// <para><b>Windows NT/2000/XP:</b> <i>dwItem2</i> contains the index
        /// in the system image list that has changed.
        /// <i>dwItem1</i> is not used and should be <see langword="null"/>.</para>
        /// <para><b>Windows 95/98:</b> <i>dwItem1</i> contains the index
        /// in the system image list that has changed.
        /// <i>dwItem2</i> is not used and should be <see langword="null"/>.</para>
        /// </remarks>
        [Flags]
        enum HChangeNotifyEventID
        {
            /// <summary>
            /// All events have occurred.
            /// </summary>
            SHCNE_ALLEVENTS = 0x7FFFFFFF,

            /// <summary>
            /// A file type association has changed. <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/>
            /// must be specified in the <i>uFlags</i> parameter.
            /// <i>dwItem1</i> and <i>dwItem2</i> are not used and must be <see langword="null"/>.
            /// </summary>
            SHCNE_ASSOCCHANGED = 0x08000000,

            /// <summary>
            /// The attributes of an item or folder have changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the item or folder that has changed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_ATTRIBUTES = 0x00000800,

            /// <summary>
            /// A nonfolder item has been created.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the item that was created.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_CREATE = 0x00000002,

            /// <summary>
            /// A nonfolder item has been deleted.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the item that was deleted.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_DELETE = 0x00000004,

            /// <summary>
            /// A drive has been added.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive that was added.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_DRIVEADD = 0x00000100,

            /// <summary>
            /// A drive has been added and the Shell should create a new window for the drive.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive that was added.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_DRIVEADDGUI = 0x00010000,

            /// <summary>
            /// A drive has been removed. <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive that was removed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_DRIVEREMOVED = 0x00000080,

            /// <summary>
            /// Not currently used.
            /// </summary>
            SHCNE_EXTENDED_EVENT = 0x04000000,

            /// <summary>
            /// The amount of free space on a drive has changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive on which the free space changed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_FREESPACE = 0x00040000,

            /// <summary>
            /// Storage media has been inserted into a drive.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive that contains the new media.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_MEDIAINSERTED = 0x00000020,

            /// <summary>
            /// Storage media has been removed from a drive.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive from which the media was removed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_MEDIAREMOVED = 0x00000040,

            /// <summary>
            /// A folder has been created. <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/>
            /// or <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that was created.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_MKDIR = 0x00000008,

            /// <summary>
            /// A folder on the local computer is being shared via the network.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that is being shared.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_NETSHARE = 0x00000200,

            /// <summary>
            /// A folder on the local computer is no longer being shared via the network.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that is no longer being shared.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_NETUNSHARE = 0x00000400,

            /// <summary>
            /// The name of a folder has changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the previous pointer to an item identifier list (PIDL) or name of the folder.
            /// <i>dwItem2</i> contains the new PIDL or name of the folder.
            /// </summary>
            SHCNE_RENAMEFOLDER = 0x00020000,

            /// <summary>
            /// The name of a nonfolder item has changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the previous PIDL or name of the item.
            /// <i>dwItem2</i> contains the new PIDL or name of the item.
            /// </summary>
            SHCNE_RENAMEITEM = 0x00000001,

            /// <summary>
            /// A folder has been removed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that was removed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_RMDIR = 0x00000010,

            /// <summary>
            /// The computer has disconnected from a server.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the server from which the computer was disconnected.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_SERVERDISCONNECT = 0x00004000,

            /// <summary>
            /// The contents of an existing folder have changed,
            /// but the folder still exists and has not been renamed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that has changed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// If a folder has been created, deleted, or renamed, use SHCNE_MKDIR, SHCNE_RMDIR, or
            /// SHCNE_RENAMEFOLDER, respectively, instead.
            /// </summary>
            SHCNE_UPDATEDIR = 0x00001000,

            /// <summary>
            /// An image in the system image list has changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_DWORD"/> must be specified in <i>uFlags</i>.
            /// </summary>
            SHCNE_UPDATEIMAGE = 0x00008000,

        }

        /// <summary>
        /// Flags that indicate the meaning of the <i>dwItem1</i> and <i>dwItem2</i> parameters.
        /// The uFlags parameter must be one of the following values.
        /// </summary>
        [Flags]
        public enum HChangeNotifyFlags
        {
            /// <summary>
            /// The <i>dwItem1</i> and <i>dwItem2</i> parameters are DWORD values.
            /// </summary>
            SHCNF_DWORD = 0x0003,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of ITEMIDLIST structures that
            /// represent the item(s) affected by the change.
            /// Each ITEMIDLIST must be relative to the desktop folder.
            /// </summary>
            SHCNF_IDLIST = 0x0000,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings of
            /// maximum length MAX_PATH that contain the full path names
            /// of the items affected by the change.
            /// </summary>
            SHCNF_PATHA = 0x0001,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings of
            /// maximum length MAX_PATH that contain the full path names
            /// of the items affected by the change.
            /// </summary>
            SHCNF_PATHW = 0x0005,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings that
            /// represent the friendly names of the printer(s) affected by the change.
            /// </summary>
            SHCNF_PRINTERA = 0x0002,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings that
            /// represent the friendly names of the printer(s) affected by the change.
            /// </summary>
            SHCNF_PRINTERW = 0x0006,
            /// <summary>
            /// The function should not return until the notification
            /// has been delivered to all affected components.
            /// As this flag modifies other data-type flags, it cannot by used by itself.
            /// </summary>
            SHCNF_FLUSH = 0x1000,
            /// <summary>
            /// The function should begin delivering notifications to all affected components
            /// but should return as soon as the notification process has begun.
            /// As this flag modifies other data-type flags, it cannot by used by itself.
            /// </summary>
            SHCNF_FLUSHNOWAIT = 0x2000
        }

        

        #endregion

        public static void UpdateShell()
        {
            // TODO: do this only after writing
            SHChangeNotify(HChangeNotifyEventID.SHCNE_ASSOCCHANGED, HChangeNotifyFlags.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        private static REGKEY _ReadClasses(string path) { return Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(path,false); }

        private static REGKEY _WriteClasses(string path, bool allUsers) { return (allUsers ? Microsoft.Win32.Registry.LocalMachine : Microsoft.Win32.Registry.CurrentUser).OpenSubKey("Software\\Classes\\" + path, true); }
        

        private readonly bool _ForAllUsers = false;

        private REGKEY _OpenKey(string path, bool writable)
        {
            if (_ForAllUsers) return Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path, writable);

            return Microsoft.Win32.Registry.CurrentUser.OpenSubKey(path, writable);
        }

                        

        public sealed class ClassExtensionView : IDisposable
        {
            #region lifecycle

            // https://msdn.microsoft.com/en-us/library/windows/desktop/cc144148%28v=vs.85%29.aspx

            private ClassExtensionView(Microsoft.Win32.RegistryKey baseKey, bool allUsers) { _BaseKey = baseKey; _AllUsers = allUsers; }

            public static ClassExtensionView Read(string extension) { return new ClassExtensionView(_ReadClasses(extension),true); }

            public static ClassExtensionView ReadSystem(string extension) { return new ClassExtensionView(_ReadClasses("SystemFileAssociations\\" + extension),true); }

            public static ClassExtensionView Write(string extension, bool allUsers) { return new ClassExtensionView(_WriteClasses(extension, allUsers), allUsers); }

            public static ClassExtensionView WriteSystem(string extension, bool allUsers) { return new ClassExtensionView(_WriteClasses("SystemFileAssociations\\" + extension, allUsers), allUsers); }

            public void Dispose()
            {
                if (_BaseKey != null)
                {
                    _BaseKey.Dispose(); _BaseKey = null;

                    // TODO: do this only after writing
                    SHChangeNotify(HChangeNotifyEventID.SHCNE_ASSOCCHANGED, HChangeNotifyFlags.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);                    
                }
            }

            #endregion

            #region data

            private Microsoft.Win32.RegistryKey _BaseKey;
            private bool _AllUsers;

            #endregion

            #region properties

            // http://www.pinvoke.net/default.aspx/shell32.shchangenotify

            public string Default { get { return _BaseKey.GetValue(null) as string; } set { _BaseKey.SetValue(null, value); } }

            public ClassExtensionApplicationView OpenApplicationKey() { return string.IsNullOrWhiteSpace(Default) ? null : ClassExtensionApplicationView.Read(Default); }

            public ClassExtensionApplicationView WriteApplicationKey() { return string.IsNullOrWhiteSpace(Default) ? null : ClassExtensionApplicationView.Write(Default, _AllUsers); }

            public string DefaultIcon { get { return _BaseKey.OpenSubKey("DefaultIcon").GetValue(null) as string; } set { _BaseKey.CreateSubKey("DefaultIcon").SetValue(null, value); } }

            public string ContentType { get { return _BaseKey.GetValue("Content Type") as string; } set { _BaseKey.SetValue("Content Type", value); } }

            // OpenWithList = (Obsolete) DO NOT USE

            public IEnumerable<string> OpenWith { get { using (var idsKey = _BaseKey.OpenSubKey("OpenWithProgIds")) { return idsKey.GetValueNames(); } } }

            public void AddOpenWithApplication(string appName)
            {
                using (var idsKey = _BaseKey.CreateSubKey("OpenWithProgIds"))
                {
                    idsKey.CreateSubKey(appName);
                }            
            }

            public ShellView OpenShell() { return new ShellView(_BaseKey.OpenSubKey("shell")); }

            public ShellView WriteShell() { return new ShellView(_BaseKey.CreateSubKey("shell")); }

            #endregion
        }

        public sealed class ShellView : IDisposable
        {
            #region lifecycle

            // https://msdn.microsoft.com/en-us/library/windows/desktop/cc144148%28v=vs.85%29.aspx

            internal ShellView(Microsoft.Win32.RegistryKey baseKey) { _BaseKey = baseKey; }            

            public void Dispose() { if (_BaseKey != null) { _BaseKey.Dispose(); _BaseKey = null; } }

            #endregion

            #region data

            private Microsoft.Win32.RegistryKey _BaseKey;

            #endregion

            #region API

            public IEnumerable<string> Verbs { get { return _BaseKey.GetSubKeyNames(); } }

            public void SerVerbCommand(string verb, string command)
            {
                using(var verbKey = _BaseKey.CreateSubKey(verb))
                {
                    using (var cmdKey = verbKey.CreateSubKey("Command"))
                    {
                        cmdKey.SetValue(null, command);
                    }
                }
            }

            #endregion
        }       

        public sealed class ClassExtensionApplicationView : IDisposable
        {
            #region lifecycle

            // https://msdn.microsoft.com/en-us/library/windows/desktop/cc144148%28v=vs.85%29.aspx

            private ClassExtensionApplicationView(Microsoft.Win32.RegistryKey baseKey) { _BaseKey = baseKey; }

            public static ClassExtensionApplicationView Read(string appKey) { return new ClassExtensionApplicationView(_ReadClasses(appKey)); }

            public static ClassExtensionApplicationView Write(string appKey, bool allUsers) { return new ClassExtensionApplicationView(_WriteClasses(appKey, allUsers)); }            

            public void Dispose()
            {
                if (_BaseKey != null)
                {
                    _BaseKey.Dispose(); _BaseKey = null;

                    // TODO: do this only after writing
                    SHChangeNotify(HChangeNotifyEventID.SHCNE_ASSOCCHANGED, HChangeNotifyFlags.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
                }
            }

            #endregion

            #region data

            private Microsoft.Win32.RegistryKey _BaseKey;

            #endregion

            #region properties

            // http://www.pinvoke.net/default.aspx/shell32.shchangenotify

            public string Default { get { return _BaseKey.GetValue(null) as string; } set { _BaseKey.SetValue(null, value); } }            

            public string DefaultIcon { get { return _BaseKey.OpenSubKey("DefaultIcon").GetValue(null) as string; } set { _BaseKey.CreateSubKey("DefaultIcon").SetValue(null, value); } }

            public ShellView OpenShell() { return new ShellView(_BaseKey.OpenSubKey("shell")); }

            #endregion
        }

        public sealed class ClassApplicationView : IDisposable
        {
            #region lifecycle

            // https://msdn.microsoft.com/en-us/library/windows/desktop/cc144148%28v=vs.85%29.aspx

            private ClassApplicationView(Microsoft.Win32.RegistryKey baseKey) { _BaseKey = baseKey; }

            public static ClassApplicationView Read(string exeFile) { return new ClassApplicationView(_ReadClasses( "Applications\\"+ exeFile)); }

            public static ClassApplicationView Write(string exeFile, bool allUsers) { return new ClassApplicationView(_WriteClasses("Applications\\" + exeFile, allUsers)); }

            public void Dispose() { if (_BaseKey != null) { _BaseKey.Dispose(); _BaseKey = null; } }

            #endregion

            #region data

            private Microsoft.Win32.RegistryKey _BaseKey;

            #endregion
        }


        


        public void RegisterApplication(string appPath)
        {
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ee872121%28v=vs.85%29.aspx

            //Finding an Application Executable

            //When the ShellExecuteEx function is called with the name of an executable file in its lpFile parameter, there are several places where the function looks for the file.
            // We recommend registering your application in the App Paths registry subkey. Doing so avoids the need for applications to modify the system PATH environment variable.

            //The file is sought in the following locations:

            //    The current working directory.
            //    The Windows directory only (no subdirectories are searched).
            //    The Windows\System32 directory.
            //    Directories listed in the PATH environment variable.
            //    Recommended: HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths


            if (!System.IO.File.Exists(appPath)) return;

            string appName = System.IO.Path.GetFileName(appPath);



            using (var appsKey = _OpenKey("Software\\Microsoft\\Windows\\CurrentVersion\\App Paths",true))
            {
                using (var exeKey = appsKey.CreateSubKey(appName))
                {
                    exeKey.SetValue(null, appPath);
                }
            }            
        }        

    }
}
