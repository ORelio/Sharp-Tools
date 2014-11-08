using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace SharpTools
{
    /// <summary>
    /// Manage screen display modes and take screenshots
    /// By ORelio - (c) 2014 - Available under the CDDL-1.0 license
    /// </summary>

    sealed class ScreenAPI
    {
        /// <summary>
        /// Represents a screen display mode: resolution, color depth, frequency, etc.
        /// </summary>

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public short dmOrientation;
            public short dmPaperSize;
            public short dmPaperLength;
            public short dmPaperWidth;
            public short dmScale;
            public short dmCopies;
            public short dmDefaultSource;
            public short dmPrintQuality;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmUnusedPadding;
            public short dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
        }

        /// <summary>
        /// Represents a display device, since Windows can handle multiple screens at the same time.
        /// </summary>

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAY_DEVICE
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;

            public DISPLAY_DEVICE(int flags)
            {
                cb = 0;
                StateFlags = flags;
                DeviceName = new string((char)32, 32);
                DeviceString = new string((char)32, 128);
                DeviceID = new string((char)32, 128);
                DeviceKey = new string((char)32, 128);
                cb = Marshal.SizeOf(this);
            }
        }

        #region Internal methods

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        [DllImport("User32.dll")]
        private static extern bool EnumDisplayDevices(IntPtr lpDevice, int iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, int dwFlags);

        [DllImport("User32.dll")]
        private static extern bool EnumDisplaySettings(string devName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        private static int devices_main = -1;
        private static Dictionary<int, DISPLAY_DEVICE> devices;
        private static Dictionary<int, List<DEVMODE>> modes;

        private static void EnumDevices()
        {
            devices.Clear();
            DISPLAY_DEVICE d = new DISPLAY_DEVICE(0);
            int devNum = 0;
            bool result;
            do
            {
                result = EnumDisplayDevices(IntPtr.Zero, devNum, ref d, 0);
                if (result)
                {
                    devices[devNum] = d;
                    if ((d.StateFlags & 4) != 0)
                        devices_main = devNum;
                }
                devNum++;
            } while (result);
        }

        private static string GetDeviceName(int devNum)
        {
            DISPLAY_DEVICE d = new DISPLAY_DEVICE(0);
            bool result = EnumDisplayDevices(IntPtr.Zero, devNum, ref d, 0);
            return (result ? d.DeviceName.Trim() : "#error#");
        }

        private static DEVMODE GetDevmode(int devNum, int modeNum)
        {
            DEVMODE devMode = new DEVMODE();
            string devName = GetDeviceName(devNum);
            EnumDisplaySettings(devName, modeNum, ref devMode);
            return devMode;
        }

        private static void EnumModes(int devNum)
        {
            if (!modes.ContainsKey(devNum)) { modes.Add(devNum, new List<DEVMODE>()); }
            modes[devNum].Clear();
            string devName = GetDeviceName(devNum);
            DEVMODE devMode = new DEVMODE();
            int modeNum = 0;
            bool result = true;
            do
            {
                result = EnumDisplaySettings(devName, modeNum, ref devMode);
                if (result) { modes[devNum].Add(devMode); }
                modeNum++;
            } while (result);
        }

        private static void initData()
        {
            devices = new Dictionary<int, DISPLAY_DEVICE>();
            modes = new Dictionary<int, List<DEVMODE>>();
            EnumDevices();
            foreach (KeyValuePair<int, DISPLAY_DEVICE> device in devices)
                EnumModes(device.Key);
        }

        #endregion

        /// <summary>
        /// Get on-screen pixel color at given coordinates
        /// </summary>
        /// <param name="x">Pixel X</param>
        /// <param name="y">Pixel Y</param>
        /// <returns>Returns the requested pixel color</returns>

        public static System.Drawing.Color GetPixelColor(int x, int y)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            uint pixel = GetPixel(hdc, x, y);
            ReleaseDC(IntPtr.Zero, hdc);
            Color color = Color.FromArgb((int)(pixel & 0x000000FF),
                         (int)(pixel & 0x0000FF00) >> 8,
                         (int)(pixel & 0x00FF0000) >> 16);
            return color;
        }

        /// <summary>
        /// Compares every pixel of two bitmaps to determine if they are equals.
        /// This method can be used to detect changes on screen using two screenshots.
        /// </summary>
        /// <param name="reference">First bitmap</param>
        /// <param name="tocompare">Second bitmap</param>
        /// <returns>TRUE if the bitmaps are equals</returns>

        public static bool compareBitmaps(Bitmap reference, Bitmap tocompare)
        {
            for (int x = 0; x < reference.Width; x++)
            {
                for (int y = 0; y < reference.Height; y++)
                {
                    if (reference.GetPixel(x, y) != tocompare.GetPixel(x, y))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Take a screenshot of a certain region of the screen
        /// </summary>
        /// <param name="x">top-left pixel X</param>
        /// <param name="y">top-left pixel Y</param>
        /// <param name="width">Rectangle width (0 = screen width)</param>
        /// <param name="height">Rectangle height (0 = screen height)</param>
        /// <returns>Returns the requested screen region</returns>

        public static Bitmap takeScreenshot(int x = 0, int y = 0, int width = 0, int height = 0)
        {
            if (width == 0) { width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width; }
            if (height == 0) { height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height; }
            Bitmap screenShotBMP = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics screenShotGraphics = Graphics.FromImage(screenShotBMP);
            screenShotGraphics.CopyFromScreen(x, y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            screenShotGraphics.Dispose();
            return screenShotBMP;
        }

        /// <summary>
        /// Find an element on the screen and returns the coordinates of its top-left pixel.
        /// If nothing was found, (-1, -1) is returned. Allows to locate a control on screen.
        /// </summary>
        /// <param name="reference">bitmap representing the control to find</param>
        /// <param name="zone">search on a specific region of the screen</param>
        /// <returns>the control's coordinates or null if not found</returns>

        public static Point? findBitmap(Bitmap reference, Rectangle? zone = null)
        {
            Bitmap screen = zone.HasValue
                ? takeScreenshot(zone.Value.X, zone.Value.Y, zone.Value.Width, zone.Value.Height)
                : takeScreenshot();

            if (screen.Width >= reference.Width && screen.Height >= reference.Height)
            {
                for (int i = 0; i + reference.Width < screen.Width; i++)
                {
                    for (int j = 0; j + reference.Height < screen.Height; j++)
                    {
                        bool location_match = true;

                        for (int x = 0; x < reference.Width && location_match; x++)
                        {
                            for (int y = 0; y < reference.Height && location_match; y++)
                            {
                                if (reference.GetPixel(x, y) != screen.GetPixel(i + x, j + y))
                                {
                                    location_match = false;
                                }
                            }
                        }

                        if (location_match)
                        {
                            screen.Dispose();
                            return new Point(i, j);
                        }
                    }
                }
            }

            screen.Dispose();
            return null;
        }

        /// <summary>
        /// Check if the specified bitmap is on the right position of the screen
        /// </summary>
        /// <param name="reference">Reference bitmap</param>
        /// <param name="position">Position</param>

        public static bool checkBitmap(Bitmap reference, Point position)
        {
            Bitmap screenBitmap = takeScreenshot(position.X, position.Y, reference.Width, reference.Height);
            for (int x = 0; x < reference.Width; x++)
            {
                for (int y = 0; y < reference.Height; y++)
                {
                    if (reference.GetPixel(x, y) != screenBitmap.GetPixel(x, y))
                    {
                        screenBitmap.Dispose();
                        return false;
                    }
                }
            }
            screenBitmap.Dispose();
            return true;
        }

        /// <summary>
        /// Get the current display mode of the main display device (throws NullReferenceException for device not found)
        /// </summary>
        /// <exception cref="NullReferenceException">When device was not found</exception>
        /// <returns>Returns the requested display mode</returns>

        public static DEVMODE MainScreen_getCurrentMode()
        {
            if (devices == null)
                initData();
            if (devices_main != -1)
            {
                return GetDevmode(devices_main, -1);
            }
            else throw new NullReferenceException("Cannot find main display device!");
        }

        /// <summary>
        /// Change the current display mode of the main display device
        /// </summary>
        /// <param name="mode">mode to apply</param>

        public static void MainScreen_setCurrentMode(DEVMODE mode)
        {
            if (mode.dmBitsPerPel != 0 & mode.dmPelsWidth != 0 & mode.dmPelsHeight != 0)
            {
                ChangeDisplaySettings(ref mode, 0);
            }
        }

        /// <summary>
        /// Check if the main display device exists and can handle the specified resolution, color depth and refresh frequency
        /// </summary>
        /// <param name="width">Display width</param>
        /// <param name="height">Display height</param>
        /// <param name="colordepth">Color depth</param>
        /// <param name="frequency">Refresh frequency</param>
        /// <returns>TRUE if a matching display mode exists</returns>

        public static bool MainScreen_canHandleMode(int width, int height, int colordepth, int frequency)
        {
            if (devices == null)
                initData();
            if (devices_main != -1)
            {
                foreach (DEVMODE mode in modes[devices_main])
                {
                    if (mode.dmPelsWidth == width
                     && mode.dmPelsHeight == height
                     && mode.dmBitsPerPel == colordepth
                     && mode.dmDisplayFrequency == frequency)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get a specific display mode for the main display device (throws NullReferenceException if not found)
        /// </summary>
        /// <param name="width">Display width</param>
        /// <param name="height">Display height</param>
        /// <param name="colordepth">Color depth</param>
        /// <param name="frequency">Refresh frequency</param>
        /// <returns>Returns the requested display mode</returns>

        public static DEVMODE MainScreen_getMode(int width, int height, int colordepth, int frequency)
        {
            if (devices == null)
                initData();
            if (devices_main != -1)
            {
                foreach (DEVMODE mode in modes[devices_main])
                {
                    if (mode.dmPelsWidth == width
                     && mode.dmPelsHeight == height
                     && mode.dmBitsPerPel == colordepth
                     && mode.dmDisplayFrequency == frequency)
                        return mode;
                }
            }
            throw new NullReferenceException("Cannot find the requested mode!");
        }
    }
}