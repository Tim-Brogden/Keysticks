/******************************************************************************
 *
 * Copyright 2019 Tim Brogden
 * All rights reserved. This program and the accompanying materials   
 * are made available under the terms of the Eclipse Public License v1.0  
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *           
 * Contributors: 
 * Tim Brogden - Keysticks application and installer
 *
 *****************************************************************************/
using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Keysticks.Sys
{
    public delegate bool CallBackPtr(IntPtr hwnd, IntPtr lParam);

    /// <summary>
    /// Windows API imports
    /// </summary>
    public class WindowsAPI
    {
        public const int WM_COPYDATA = 0x004A;

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);

        // Map types for MapVirtualKey
        public const uint MAPVK_VK_TO_VSC = 0u;
        public const uint MAPVK_VSC_TO_VK = 1u;
        public const uint MAPVK_VK_TO_CHAR = 2u;
        public const uint MAPVK_VSC_TO_VK_EX = 3u;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int ToUnicodeEx(uint virtualKey,
                                            uint scanCode,
                                            byte[] keyStates,
                                            [MarshalAs(UnmanagedType.LPArray)] [Out] char[] chars,
                                            int charMaxCount,
                                            uint flags,
                                            IntPtr dwhkl);

        public const int KL_NAMELENGTH = 9;

        [DllImport("user32.dll")]
        public static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        public static extern uint SendInput(uint numberOfInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] input, int structSize);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int EnumWindows(CallBackPtr callPtr, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int EnumChildWindows(IntPtr hWnd, CallBackPtr callPtr, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumThreadWindows(uint dwThreadId, CallBackPtr lpfn, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        public static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

        //[DllImport("user32.dll")]
        //public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int GWL_EXSTYLE = -20;

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);
        
        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        public const int INPUT_MOUSE = 0;
        public const int INPUT_KEYBOARD = 1;
        public const int INPUT_HARDWARE = 2;
        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const uint KEYEVENTF_KEYUP = 0x0002;
        public const uint KEYEVENTF_UNICODE = 0x0004;
        public const uint KEYEVENTF_SCANCODE = 0x0008;
        public const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        public const uint MOUSEEVENTF_HWHEEL = 0x01000;
        public const uint MOUSEEVENTF_MOVE = 0x0001;
        public const uint MOUSEEVENTF_MOVE_NOCOALESCE = 0x2000;
        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;
        public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        public const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        public const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        public const uint MOUSEEVENTF_VIRTUALDESK = 0x4000;
        public const uint MOUSEEVENTF_WHEEL = 0x0800;
        public const uint MOUSEEVENTF_XDOWN = 0x0080;
        public const uint MOUSEEVENTF_XUP = 0x0100;
        public const int WHEEL_DELTA = 120;
        public const uint XBUTTON1 = 0x0001;
        public const uint XBUTTON2 = 0x0002;

        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;

        public const uint SW_HIDE = 0;
        public const uint SW_SHOWNORMAL = 1;
        public const uint SW_SHOWMINIMIZED = 2;
        public const uint SW_SHOWMAXIMIZED = 3;
        public const uint SW_MAXIMIZE = 3;
        public const uint SW_SHOWNOACTIVATE = 4;
        public const uint SW_SHOW = 5;
        public const uint SW_MINIMIZE = 6;
        public const uint SW_SHOWMINNOACTIVE = 7;
        public const uint SW_SHOWNA = 8;
        public const uint SW_RESTORE = 9;

        public const uint WS_BORDER = 0x00800000;
        public const uint WS_CAPTION = 0x00C00000;
        public const uint WS_CHILD = 0x40000000;
        public const uint WS_CHILDWINDOW = 0x40000000;
        public const uint WS_CLIPCHILDREN = 0x02000000;
        public const uint WS_CLIPSIBLINGS = 0x04000000;
        public const uint WS_DISABLED = 0x08000000;
        public const uint WS_DLGFRAME = 0x00400000;
        public const uint WS_GROUP = 0x00020000;
        public const uint WS_HSCROLL = 0x00100000;
        public const uint WS_ICONIC = 0x20000000;
        public const uint WS_MAXIMIZE = 0x01000000;
        public const uint WS_MAXIMIZEBOX = 0x00010000;
        public const uint WS_MINIMIZE = 0x20000000;
        public const uint WS_MINIMIZEBOX = 0x00020000;
        public const uint WS_OVERLAPPED = 0x00000000;
        public const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
        public const uint WS_POPUP = 0x80000000;
        public const uint WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU;
        public const uint WS_SIZEBOX = 0x00040000;
        public const uint WS_SYSMENU = 0x00080000;
        public const uint WS_TABSTOP = 0x00010000;
        public const uint WS_THICKFRAME = 0x00040000;
        public const uint WS_TILED = 0x00000000;
        public const uint WS_TILEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
        public const uint WS_VISIBLE = 0x10000000;
        public const uint WS_VSCROLL = 0x00200000;
        
        /// <summary>
        /// Decide which windows we can activate
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static bool IsStandardWindowStyle(uint style)
        {
            return (style & WS_SYSMENU) != 0 &&
                    (style & WindowsAPI.WS_VISIBLE) != 0 &&
                    (style & WindowsAPI.WS_DISABLED) == 0;
        }

        /// <summary>
        /// Force window minimise
        /// </summary>
        /// <param name="hForegroundWnd"></param>
        /// <param name="restoreIfMinimised"></param>
        public static void ForceMinimiseWindow(IntPtr hForegroundWnd)
        {
            bool forced = false;

            // Check that the window can be minimised
            if ((GetWindowStyle(hForegroundWnd) & WS_MINIMIZEBOX) != 0)
            {
                //if (Environment.OSVersion.Version.Major >= 6)
                //{
                    uint foreThreadID = GetWindowThreadProcessId(hForegroundWnd, IntPtr.Zero);
                    uint appThreadID = (uint)AppDomain.GetCurrentThreadId();

                    if (foreThreadID != appThreadID)
                    {
                        if (AttachThreadInput(foreThreadID, appThreadID, true))
                        {
                            ShowWindow(hForegroundWnd, SW_MINIMIZE);
                            AttachThreadInput(foreThreadID, appThreadID, false);

                            forced = true;
                        }
                    }
                //}

                if (!forced)
                {
                    // No force required
                    ShowWindow(hForegroundWnd, SW_MINIMIZE);
                }
            }
        }

        /// <summary>
        /// Force activation of a window
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="restoreIfMinimised"></param>
        public static void ForceActivateWindow(IntPtr hWnd, bool restoreIfMinimised, bool minimiseIfActive)
        {           
            IntPtr foreWindow = GetForegroundWindow();
            if (foreWindow != hWnd)
            {
                bool forced = false;
                //if (Environment.OSVersion.Version.Major >= 6)
                //{
                    uint foreThreadID = 0u;
                    if (foreWindow != IntPtr.Zero)
                    {
                        foreThreadID = GetWindowThreadProcessId(foreWindow, IntPtr.Zero);
                    }
                    uint appThreadID = (uint)AppDomain.GetCurrentThreadId();

                    if (foreThreadID != appThreadID)
                    {
                        if (AttachThreadInput(foreThreadID, appThreadID, true))
                        {
                            ActivateWindow(hWnd, restoreIfMinimised);
                            AttachThreadInput(foreThreadID, appThreadID, false);

                            forced = true;
                        }
                    }
                //}

                if (!forced)
                {
                    // No force required
                    ActivateWindow(hWnd, restoreIfMinimised);
                }
            }
            else if (minimiseIfActive)
            {
                // Specified window is already the active window and can be minimised
                ForceMinimiseWindow(hWnd);
            }
        }

        /// <summary>
        /// Activate a window normally
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="restoreIfMinimised"></param>
        private static void ActivateWindow(IntPtr hWnd, bool restoreIfMinimised)
        {
            uint showState = GetWindowShowState(hWnd);
            BringWindowToTop(hWnd);
            if (restoreIfMinimised && showState == SW_SHOWMINIMIZED)
            {
                ShowWindow(hWnd, SW_RESTORE);
            }
            else
            {
                ShowWindow(hWnd, SW_SHOW);
            }
        }

        /// <summary>
        /// Get the state of a window
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static uint GetWindowShowState(IntPtr hWnd)
        {
            uint showState = SW_HIDE;
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = (uint)Marshal.SizeOf(placement);
            if (GetWindowPlacement(hWnd, ref placement))
            {
                showState = placement.showCmd;
            }

            return showState;
        }

        /// <summary>
        /// Get the style of a window
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static uint GetWindowStyle(IntPtr hWnd)
        {
            uint style = 0u;
            WINDOWINFO info = new WINDOWINFO(true);
            if (GetWindowInfo(hWnd, ref info))
            {
                style = info.dwStyle;
            }

            return style;
        }

        /// <summary>
        /// Get the title of a window
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static string GetTitleBarText(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);

            return sb.ToString();
        }

        public static void SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            IntPtr result = IntPtr.Zero;

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
            }
            else
            {
                // use SetWindowLongPtr
                IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public RECT(RECT rectangle)
        {
            Left = rectangle.Left;
            Top = rectangle.Top;
            Right = rectangle.Right;
            Bottom = rectangle.Bottom;
        }

        public bool Equals(RECT rect)
        {
            return Left == rect.Left && Top == rect.Top && Right == rect.Right && Bottom == rect.Bottom;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HARDWAREINPUT
    {
        uint uMsg;
        ushort wParamL;
        ushort wParamH;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct INPUT
    {
        [FieldOffset(0)]
        public int type;
        [FieldOffset(4)]
        public MOUSEINPUT mi;
        [FieldOffset(4)]
        public KEYBDINPUT ki;
        [FieldOffset(4)]
        public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public Int32 x;
        public Int32 y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public uint length;
        public uint flags;
        public uint showCmd;
        public POINT ptMinPosition;
        public POINT ptMaxPosition;
        public RECT rcNormalPosition;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWINFO
    {
        public uint cbSize;
        public RECT rcWindow;
        public RECT rcClient;
        public uint dwStyle;
        public uint dwExStyle;
        public uint dwWindowStatus;
        public uint cxWindowBorders;
        public uint cyWindowBorders;
        public ushort atomWindowType;
        public ushort wCreatorVersion;

        public WINDOWINFO(Boolean? filler)
            : this()   // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
        {
            cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
        }
    }
}