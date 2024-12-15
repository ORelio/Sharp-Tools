using System;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace SharpTools
{
    /// <summary>
    /// Monitor session events: startup, logon, logoff, shutdown.
    /// Works using a hidden form as shutdown block reason requires a valid window handle.
    /// By ORelio (c) 2028-2024 - Available under the CDDL-1.0 license
    /// </summary>
    /// <remarks>
    /// This class is a simplified version of:
    /// https://github.com/ORelio/Sound-Manager/blob/master/SoundManager/BgSoundPlayer.cs
    /// 
    /// Basic usage:
    /// 1. Reference and Import Windows Forms
    /// using System.Windows.Forms;
    /// 
    /// 2. Define a callback
    /// void MyEventCallback(EventMonitorForm.SessionEvent eventType) { /* */ }
    /// 
    /// 3. Run the hidden form
    /// Application.Run(new EventMonitorForm(MyEventCallback, "My cool App", "I still need to do something"));
    /// Note: Application.Run() is blocking until the hidden form is closed by logoff or shutdown. Use a thread if needed.
    /// 
    /// 4. Your callback will receive events as they occur:
    /// - It is assumed that your app will run on startup, so your callback will immediately receive startup and/or logon event
    /// - On logon, sends startup on first launch, then logon always
    /// - On system lock or user account switch, sends logoff / logon accordingly
    /// - On logoff, the sends logoff always, and shutdown if the logoff reason is shutdown
    /// - The form will automatically block system logoff/shutdown until your callback returns
    /// </remarks>
    class SessionEventMonitor : Form
    {
        public static readonly string ExeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        public static readonly string LastBootFile = Path.Combine(ExeDir, "LastBootTime.ini");

        /// <summary>
        /// Set of supported system events
        /// </summary>
        public enum SessionEvent
        {
            Startup,
            Logon,
            Logoff,
            Shutdown
        }

        [DllImport("kernel32")]
        private static extern UInt64 GetTickCount64();

        [DllImport("user32.dll")]
        private static extern bool ShutdownBlockReasonCreate(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] string pwszReason);

        [DllImport("user32.dll")]
        private static extern bool ShutdownBlockReasonDestroy(IntPtr wndHandle);

        /// <summary>
        /// Callback when a system event occurs
        /// </summary>
        private Action<SessionEvent> eventCallback;

        /// <summary>
        /// Window title shown in system shutdown event
        /// </summary>
        private string windowTitle;

        /// <summary>
        /// Message shown by the system when app blocks shutdown
        /// </summary>
        private string shutdownBlockReason;

        /// <summary>
        /// Get boot unix timestamp in seconds
        /// </summary>
        private static long GetBootTimestamp()
        {
            long remainder;
            return Math.DivRem(((long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds - (long)GetTickCount64()), 1000, out remainder);
        }

        /// <summary>
        /// Create a new system event monitor form using the specified callback
        /// </summary>
        /// <param name="eventCallback">Function to call with events</param>
        /// <param name="windowTitle">Window title shown in system shutdown event</param>
        /// <param name="shutdownBlockReason">Message shown by the system when app blocks shutdown while calling callback</param>
        public SessionEventMonitor(Action<SessionEvent> eventCallback, string windowTitle, string shutdownBlockReason)
        {
            this.eventCallback = eventCallback;
            this.windowTitle = windowTitle;
            this.shutdownBlockReason = shutdownBlockReason;
            this.Icon = IconExtractor.ExtractAssociatedIcon(Application.ExecutablePath);

            // Hide the window in such a way Windows will still consider it eligible for ShutdownBlockReason
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow; // Remove from Alt+Tab
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new System.Drawing.Point(-9999, -9999);
            this.Size = new System.Drawing.Size(1, 1);
            this.GotFocus += new EventHandler(WindowFocused); // Evade focus - See WindowFocused() below
            this.Text = "�"; // Avoid screen readers saying the window title loud - See WindowFocused() below

            HandleStartupOrLogon();
            SystemEvents.SessionEnding += new SessionEndingEventHandler(SessionEvents_SessionEnding);
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SessionEvents_SessionSwitch);
        }

        /// <summary>
        /// Handle Startup or Logon event on app launch
        /// </summary>
        private void HandleStartupOrLogon()
        {
            // Tell apart startup and logon using last system boot time
            string bootTime = GetBootTimestamp().ToString();
            bootTime = bootTime.Substring(0, bootTime.Length - 1) + '0';
            string lastBootTime = "";
            if (File.Exists(LastBootFile))
                lastBootTime = File.ReadAllText(LastBootFile);

            if (bootTime != lastBootTime)
            {
                File.WriteAllText(LastBootFile, bootTime);
                eventCallback(SessionEvent.Startup);
            }

            eventCallback(SessionEvent.Logon);
        }

        /// <summary>
        /// Detect user logging off and play the appropriate logoff/shutdown sound
        /// </summary>
        private void SessionEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            this.Text = windowTitle;
            ShutdownBlockReasonCreate(this.Handle, shutdownBlockReason);
            eventCallback(SessionEvent.Logoff);

            if (e.Reason == SessionEndReasons.SystemShutdown)
            {
                File.Delete(LastBootFile); // Windows 8+ hibernates under the hood on shutdown: make sure next launch will count as startup
                eventCallback(SessionEvent.Shutdown);
            }

            ShutdownBlockReasonDestroy(this.Handle);
        }

        /// <summary>
        /// Detect user leaving and resuming session
        /// </summary>
        private void SessionEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                eventCallback(SessionEvent.Logoff);
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                eventCallback(SessionEvent.Logon);
            }
        }

        /// <summary>
        /// Detect when the window is focused
        /// </summary>
        private void WindowFocused(object sender, EventArgs e)
        {
            // Refuse focus - Some screen readers may pick up the window
            // Focus workaround found in https://github.com/ORelio/Sound-Manager/issues/8
            // Try switching focus to desktop, or as fallback, toggle the minimized state
            try
            {
                // Cannot stay minimized because it may show a window title next to the task bar
                this.WindowState = FormWindowState.Minimized;
                this.WindowState = FormWindowState.Normal;

                // Switch focus to the Windows Desktop's folderView
                IntPtr desktop = IntPtr.Zero;
                if (WindowManager.GetDesktopWindow(ref desktop))
                    WindowManager.SetForegroundWindow(desktop);
            }
            catch
            {
                // Avoid crashes linked to this workaround
            }
        }
    }
}
