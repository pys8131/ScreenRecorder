﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenRecorder.Region
{
    public sealed class WindowRegion
    {
        #region Native Methods
        public static Rect GetWindowRectangle(IntPtr hWnd)
        {
            // including type conversation from RECT to Rect
            int size = Marshal.SizeOf(typeof(RECT));
            DwmGetWindowAttribute(hWnd, (int)DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT rect, size);
            Rect region = new Rect(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            return region;
        }

        [Flags]
        private enum DwmWindowAttribute : uint
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_LAST
        }

        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

        [DllImport("user32.DLL")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumWindows(WindowEnumProc callback, int extraData);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
        public delegate bool WindowEnumProc(IntPtr hwnd, IntPtr lparam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr hwnd, WindowEnumProc callback, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);
        #endregion

        public static void SizeWindow(IntPtr hwnd, int cx, int cy)
        {
            SizeWindow(hwnd, 0, 0, cx, cy, false);
        }

        public static void SizeWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool move)
        {
            const short SWP_NOMOVE = 0X2;
            //const short SWP_NOSIZE = 1;
            const short SWP_NOZORDER = 0X4;
            const int SWP_SHOWWINDOW = 0x0040;

            int flags = SWP_NOZORDER | SWP_SHOWWINDOW;
            if (!move)
            {
                flags |= SWP_NOMOVE;
                x = 0;
                y = 0;
            }

            SetWindowPos(hwnd, 0, x, y, cx, cy, flags);
        }

        public Rect Region { get; private set; }
        public IntPtr Hwnd { get; private set; }

        /// <summary>
        /// Get the windows you see on the screen from the front.
        /// </summary>
        /// <returns></returns>
        static public WindowRegion[] GetWindowRegions()
        {
            List<WindowRegion> windowRegions = new List<WindowRegion>();

            EnumWindows((hWnd, lparam) =>
            {
                if(IsWindowVisible(hWnd) && !Utils.IsWindowDisplayedOnlyMonitor(hWnd))
                {
                    Rect region = GetWindowRectangle(hWnd);
                    if (region.Height > 16 && region.Width > 16)
                    {
                        windowRegions.Add(new WindowRegion() { Region = region, Hwnd = hWnd });
                    }
                }
                return true;
            }, 0);

            return windowRegions.Count > 0 ? windowRegions.ToArray() : null;
        }
    }
}
