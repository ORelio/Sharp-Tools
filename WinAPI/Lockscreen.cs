using System;
using System.IO;
using SharpTools;

namespace SharpTools
{
    /// <summary>
    /// Wrapper around the Windows 10 lockscreen
    /// By ORelio (c) 2018 - CDDL 1.0
    /// </summary>
    /// <remarks>
    /// Requires ../Utility/FileSystemAdmin.cs and ../Utility/Privilege20 in order to compile.
    /// </remarks>
    class Lockscreen
    {
        /// <summary>
        /// Replace the Windows 10 global lockscreen
        /// </summary>
        /// <remarks>Administrator permissions are required</remarks>
        /// <param name="path">Path to the new .jpg file for global lockscreen</param>
        public static void SetGlobalLockscreen(string path)
        {
            string systemDataDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Microsoft\Windows\SystemData";
            string systemDataCache = systemDataDir + @"\S-1-5-18\ReadOnly\LockScreen_Z";
            string lockscreenDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\Web\Screen";
            string lockscreenPic = lockscreenDir + @"\img100.jpg";
            string lockscreenBak = lockscreenDir + @"\img200.jpg";

            FileSystemAdmin.GrantAll(systemDataDir, true);
            FileSystemAdmin.GrantAll(lockscreenDir, true);

            if (!File.Exists(lockscreenBak))
                File.Copy(lockscreenPic, lockscreenBak);
            File.Copy(path, lockscreenPic, true);

            FileSystemAdmin.DeleteContents(systemDataCache);
        }
    }
}
