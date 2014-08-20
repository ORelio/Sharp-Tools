using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace SharpTools
{
    /// <summary>
    /// Programatically trigger keypresses and mouse interactions using Windows API
    /// By ORelio - (c) 2013-2014 - Available under the CDDL-1.0 license
    /// </summary>

    public static class KeySender
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        public enum MouseButton { Left, Middle, Right, Previous, Next }

        private enum MouseEvent : uint
        {
            LEFTDOWN = 0x02,
            LEFTUP = 0x04,
            MIDDLEDOWN = 0x20,
            MIDDLEUP = 0x40,
            RIGHTDOWN = 0x08,
            RIGHTUP = 0x10,
            XDOWN = 0x80,
            XUP = 0x100,
            XFLAGPREVIOUS = 0x01,
            XFLAGNEXT = 0x02
        }

        /// <summary>
        /// Trigger a key press using key code. Use KeySender.KeyCodes to get key codes.
        /// </summary>
        /// <param name="key">Key to press</param>

        public static void KeyPress(byte key)
        {
            keybd_event(key, (byte)0x02, 0, UIntPtr.Zero);
        }

        /// <summary>
        /// Trigger a key release using key code. Use KeySender.KeyCodes to get key codes.
        /// </summary>
        /// <param name="key">Key to release</param>

        public static void KeyRelease(byte key)
        {
            keybd_event(key, (byte)0x82, (uint)0x2, UIntPtr.Zero);
        }

        /// <summary>
        /// Trigger a key press followed by a key release using key code. Use KeySender.KeyCodes to get key codes.
        /// </summary>
        /// <param name="key">Key to press and release</param>

        public static void KeyStroke(byte key)
        {
            KeyPress(key); KeyRelease(key);
        }

        /// <summary>
        /// Trigger a mouse button press
        /// </summary>
        /// <param name="B">Button to press</param>

        public static void MousePress(MouseButton B)
        {
            MousePress(B, Cursor.Position);
        }

        /// <summary>
        /// Trigger a mouse button press at a specific screen location
        /// </summary>
        /// <param name="B">Button to press</param>
        /// <param name="P">Location on screen of the button press event</param>

        public static void MousePress(MouseButton B, Point P)
        {
            Cursor.Position = P;
            uint buttoncode = 0;
            uint clicdata = 0;

            switch (B)
            {
                case MouseButton.Left: buttoncode = (uint)MouseEvent.LEFTDOWN; break;
                case MouseButton.Middle: buttoncode = (uint)MouseEvent.MIDDLEDOWN; break;
                case MouseButton.Right: buttoncode = (uint)MouseEvent.RIGHTDOWN; break;
                case MouseButton.Previous: buttoncode = (uint)MouseEvent.XDOWN; clicdata = (uint)MouseEvent.XFLAGPREVIOUS; break;
                case MouseButton.Next: buttoncode = (uint)MouseEvent.XDOWN; clicdata = (uint)MouseEvent.XFLAGNEXT; break;
            }

            mouse_event(buttoncode, (uint)P.X, (uint)P.Y, clicdata, UIntPtr.Zero);
        }

        /// <summary>
        /// Trigger a mouse button release
        /// </summary>
        /// <param name="B">Button to release</param>

        public static void MouseRelease(MouseButton B)
        {
            MouseRelease(B, Cursor.Position);
        }

        /// <summary>
        /// Trigger a mouse button release at a specific screen location
        /// </summary>
        /// <param name="B">Button to release</param>
        /// <param name="P">Location on screen of the button release event</param>

        public static void MouseRelease(MouseButton B, Point P)
        {
            Cursor.Position = P;
            uint buttoncode = 0;
            uint clicdata = 0;

            switch (B)
            {
                case MouseButton.Left: buttoncode = (uint)MouseEvent.LEFTUP; break;
                case MouseButton.Middle: buttoncode = (uint)MouseEvent.MIDDLEUP; break;
                case MouseButton.Right: buttoncode = (uint)MouseEvent.RIGHTUP; break;
                case MouseButton.Previous: buttoncode = (uint)MouseEvent.XUP; clicdata = (uint)MouseEvent.XFLAGPREVIOUS; break;
                case MouseButton.Next: buttoncode = (uint)MouseEvent.XUP; clicdata = (uint)MouseEvent.XFLAGNEXT; break;
            }

            mouse_event(buttoncode, (uint)P.X, (uint)P.Y, clicdata, UIntPtr.Zero);
        }

        /// <summary>
        /// Trigger a mouse button press and release event
        /// </summary>
        /// <param name="B">Button to press and release</param>

        public static void MouseClick(MouseButton B)
        {
            MouseClick(B, Cursor.Position);
        }

        /// <summary>
        /// Trigger a mouse button press and release at a specific screen location
        /// </summary>
        /// <param name="B">Button to press and release</param>
        /// <param name="P">Location on screen of the button press and release events</param>

        public static void MouseClick(MouseButton B, Point P)
        {
            MousePress(B, P);
            MouseRelease(B, P);
        }
        
        /// <summary>
        /// A collection of key codes for keyboard events
        /// </summary>

        public static class KeyCodes
        {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            private static extern byte VkKeyScan(char ch);

            public static byte A = VkKeyScan('a'); public static byte J = VkKeyScan('j'); public static byte S = VkKeyScan('s');
            public static byte B = VkKeyScan('b'); public static byte K = VkKeyScan('k'); public static byte T = VkKeyScan('t');
            public static byte C = VkKeyScan('c'); public static byte L = VkKeyScan('l'); public static byte U = VkKeyScan('u');
            public static byte D = VkKeyScan('d'); public static byte M = VkKeyScan('m'); public static byte V = VkKeyScan('v');
            public static byte E = VkKeyScan('e'); public static byte N = VkKeyScan('n'); public static byte W = VkKeyScan('w');
            public static byte F = VkKeyScan('f'); public static byte O = VkKeyScan('o'); public static byte X = VkKeyScan('x');
            public static byte G = VkKeyScan('g'); public static byte P = VkKeyScan('p'); public static byte Y = VkKeyScan('y');
            public static byte H = VkKeyScan('h'); public static byte Q = VkKeyScan('q'); public static byte Z = VkKeyScan('z');
            public static byte I = VkKeyScan('i'); public static byte R = VkKeyScan('r');

            public static byte NumPad0 = 0x60;
            public static byte NumPad1 = 0x61;
            public static byte NumPad2 = 0x62;
            public static byte NumPad3 = 0x63;
            public static byte NumPad4 = 0x64;
            public static byte NumPad5 = 0x65;
            public static byte NumPad6 = 0x66;
            public static byte NumPad7 = 0x67;
            public static byte NumPad8 = 0x68;
            public static byte NumPad9 = 0x69;

            public static byte NumpadMultiply = 0x6A;
            public static byte NumpadPlus = 0x6B;
            public static byte NumpadSeparator = 0x6C;
            public static byte NumpadMinus = 0x6D;
            public static byte NumpadDecimal = 0x6E;
            public static byte NumpadDivide = 0x6F;
            public static byte NumpadNumLock = 0x90;

            public static byte Scroll = 0x91;
            public static byte ShiftLeft = 0xA0;
            public static byte ShiftRight = 0xA1;
            public static byte ControlLeft = 0xA2;
            public static byte ControlRight = 0xA3;
            public static byte MenuLeft = 0xA4;
            public static byte MenuRight = 0xA5;
            public static byte Back = 0x08;
            public static byte Tab = 0x09;
            public static byte Return = 0x0D;
            public static byte Shift = 0x10;
            public static byte Control = 0x11;
            public static byte Alt = 0x12;
            public static byte Pause = 0x13;
            public static byte Capital = 0x14;
            public static byte Escape = 0x1B;
            public static byte Space = 0x20;
            public static byte End = 0x23;
            public static byte Home = 0x24;
            public static byte ArrowLeft = 0x25;
            public static byte ArrowUp = 0x26;
            public static byte ArrowRight = 0x27;
            public static byte ArrowDown = 0x28;
            public static byte PrintScreen = 0x2A;
            public static byte Snapshot = 0x2C;
            public static byte Insert = 0x2D;
            public static byte Delete = 0x2E;
            public static byte WinKeyLeft = 0x5B;
            public static byte WinKeyRight = 0x5C;

            public static byte F1 = 0x70;
            public static byte F2 = 0x71;
            public static byte F3 = 0x72;
            public static byte F4 = 0x73;
            public static byte F5 = 0x74;
            public static byte F6 = 0x75;
            public static byte F7 = 0x76;
            public static byte F8 = 0x77;
            public static byte F9 = 0x78;
            public static byte F10 = 0x79;
            public static byte F11 = 0x7A;
            public static byte F12 = 0x7B;
        }
    }
}
