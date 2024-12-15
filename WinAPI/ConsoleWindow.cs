using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

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

        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int AllocConsole();

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        private const int STD_OUTPUT_HANDLE = -11;
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
            if (IsAllocated())
                SetConsoleIcon(icon.Handle);
        }

        /// <summary>
        /// Set custom icon from .ico or .exe file
        /// </summary>
        public static void SetIcon(string iconFile)
        {
            if (IsAllocated())
                SetConsoleIcon(Icon.ExtractAssociatedIcon(iconFile).Handle);
        }

        /// <summary>
        /// Set the icon back to the default CMD icon
        /// </summary>
        public static void ResetIcon()
        {
            if (IsAllocated())
                SetConsoleIcon(Icon.ExtractAssociatedIcon(Environment.SystemDirectory + "\\cmd.exe").Handle);
        }

        /// <summary>
        /// Check if a console window is currently allocated
        /// </summary>
        /// <returns>TRUE if the console window exists</returns>
        public static bool IsAllocated()
        {
            return windowHandle != IntPtr.Zero;
        }

        /// <summary>
        /// Create console window if the application was not a console application
        /// </summary>
        public static void Allocate()
        {
            if (windowHandle == IntPtr.Zero)
            {
                AllocConsole();
                IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                Microsoft.Win32.SafeHandles.SafeFileHandle safeFileHandle = new Microsoft.Win32.SafeHandles.SafeFileHandle(stdHandle, true);
                FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
                StreamWriter standardOutput = new StreamWriter(fileStream, Console.OutputEncoding);
                standardOutput.AutoFlush = true;
                Console.SetOut(standardOutput);
            }
            windowHandle = GetConsoleWindow();
        }

        /// <summary>
        /// Hide the console window
        /// </summary>
        public static void Hide()
        {
            if (IsAllocated())
                ShowWindow(windowHandle, SW_HIDE);
        }

        /// <summary>
        /// Display the console window
        /// </summary>
        public static void Show()
        {
            if (IsAllocated())
                ShowWindow(windowHandle, SW_SHOW);
        }
    }
}
