using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SharpTools
{
    /// <summary>
    /// Allow to manage console window, on Windows only.
    /// See StackOverflow no. 2986853, 3571627
    /// </summary>
    public static class ConsoleWindow
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleIcon(IntPtr hIcon);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        private static IntPtr windowHandle = IntPtr.Zero;

        /// <summary>
        /// Static initializer: Detects Windows environment.
        /// This class has no effect outside a Windows environment.
        /// </summary>
        static ConsoleWindow()
        {
            try
            {
                windowHandle = GetConsoleWindow();
            }
            catch { /* Not on a Windows environment, or no console for this process */ }
        }

        /// <summary>
        /// Set custom icon from an Icon resource
        /// </summary>
        public static void SetIcon(Icon icon)
        {
            if (windowHandle != IntPtr.Zero)
                SetConsoleIcon(icon.Handle);
        }

        /// <summary>
        /// Set custom icon from .ico or .exe file
        /// </summary>
        public static void SetIcon(string iconFile)
        {
            if (windowHandle != IntPtr.Zero)
                SetConsoleIcon(Icon.ExtractAssociatedIcon(iconFile).Handle);
        }

        /// <summary>
        /// Set the icon back to the default CMD icon
        /// </summary>
        public static void ResetIcon()
        {
            if (windowHandle != IntPtr.Zero)
                SetConsoleIcon(Icon.ExtractAssociatedIcon(Environment.SystemDirectory + "\\cmd.exe").Handle);
        }

        /// <summary>
        /// Hide the console window
        /// </summary>
        public static void Hide()
        {
            if (windowHandle != IntPtr.Zero)
                ShowWindow(windowHandle, SW_HIDE);
        }

        /// <summary>
        /// Display the console window
        /// </summary>
        public static void Show()
        {
            if (windowHandle != IntPtr.Zero)
                ShowWindow(windowHandle, SW_SHOW);
        }
    }
}
