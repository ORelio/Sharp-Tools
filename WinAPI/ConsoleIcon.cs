using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace SharpTools
{
    /// <summary>
    /// Allow to set the console icon, on Windows only.
    /// See StackOverflow no. 2986853
    /// </summary>

    public static class ConsoleIcon
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleIcon(IntPtr hIcon);

        /// <summary>
        /// Set custom icon from an Icon resource
        /// </summary>
        
        public static void setIcon(Icon icon)
        {
            SetConsoleIcon(icon.Handle);
        }
        
        /// <summary>
        /// Set custom icon from .ico or .exe file
        /// </summary>

        public static void setIcon(string iconFile)
        {
            SetConsoleIcon(Icon.ExtractAssociatedIcon(iconFile).Handle);
        }

        /// <summary>
        /// Set the icon back to the default CMD icon
        /// </summary>

        public static void revertToCMDIcon()
        {
            SetConsoleIcon(Icon.ExtractAssociatedIcon(Environment.SystemDirectory + "\\cmd.exe").Handle);
        }
    }
}
