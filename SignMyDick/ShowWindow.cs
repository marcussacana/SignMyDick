﻿using System;
using System.Runtime.InteropServices;

namespace SignMyDick
{
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    unsafe delegate bool ShowWindowDelegate(void* hWnd, CMDShow nCmdShow);
    unsafe class ShowWindow : Hook.Hook<ShowWindowDelegate>
    {
        public override string Library => "user32.dll";

        public override string Export => "ShowWindow";

        public override void Initialize()
        {
            HookDelegate = new ShowWindowDelegate(hShowWindow);
            Compile();
        }

        bool UnderEvent = false;

        public event ShowWindowEvent BeforeShowWindow; 
        public event ShowWindowEvent AfterShowWindow;
        bool hShowWindow(void* hWnd, CMDShow nCmdShow)
        {
            if (UnderEvent)
                return Bypass(hWnd, nCmdShow);
            
            var Args = new ShowWindowEventArgs(hWnd, Win32.GetClassNameFromHandle(hWnd), Win32.GetWindowText(hWnd), nCmdShow);
            BeforeShowWindow?.Invoke(this, Args);
            nCmdShow = Args.nCmdShow;
            if (!Args.Continue)
            {
                UnderEvent = false;
                return true;
            }
            UnderEvent = false;
            var Rst = Bypass(hWnd, nCmdShow);
            AfterShowWindow?.Invoke(this, Args);
            return Rst;
        }
    }

    public enum CMDShow : int {
        /// <summary>
        /// Minimizes a window, even if the thread that owns the window is not responding. This flag should only be used when minimizing windows from a different thread.
        /// </summary>
        SW_FORCEMINIMIZE = 11,
        /// <summary>
        /// Hides the window and activates another window.
        /// </summary>
        SW_HIDE = 0,
        /// <summary>
        /// Maximizes the specified window.
        /// </summary>
        SW_MAXIMIZE = 3,
        /// <summary>
        /// Minimizes the specified window and activates the next top-level window in the Z order.
        /// </summary>
        SW_MINIMIZE = 6,
        /// <summary>
        /// Activates and displays the window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when restoring a minimized window.
        /// </summary>
        SW_RESTORE = 9,
        /// <summary>
        /// Activates the window and displays it in its current size and position.
        /// </summary>
        SW_SHOW = 5,
        /// <summary>
        /// Sets the show state based on the SW_ value specified in the STARTUPINFO structure passed to the CreateProcess function by the program that started the application.
        /// </summary>
        SW_SHOWDEFAULT = 10,
        /// <summary>
        /// Activates the window and displays it as a maximized window.
        /// </summary>
        SW_SHOWMAXIMIZED = 3,
        /// <summary>
        /// Activates the window and displays it as a minimized window.
        /// </summary>
        SW_SHOWMINIMIZED = 2,
        /// <summary>
        /// Displays the window as a minimized window. This value is similar to SW_SHOWMINIMIZED, except the window is not activated.
        /// </summary>
        SW_SHOWMINNOACTIVE = 7,
        /// <summary>
        /// Displays the window in its current size and position. This value is similar to SW_SHOW, except that the window is not activated.
        /// </summary>
        SW_SHOWNA = 8,
        /// <summary>
        /// Displays a window in its most recent size and position. This value is similar to SW_SHOWNORMAL, except that the window is not activated.
        /// </summary>
        SW_SHOWNOACTIVATE = 4,
        /// <summary>
        /// Activates and displays a window. If the window is minimized or maximized, the system restores it to its original size and position. An application should sp
        /// </summary>
        SW_SHOWNORMAL = 1
    }

    public delegate void ShowWindowEvent(object sender, ShowWindowEventArgs Args);
    public unsafe class ShowWindowEventArgs : EventArgs
    {
        public readonly void* hWnd;
        public readonly string Class;
        public readonly string Title;
        public ShowWindowEventArgs(void* hWnd, string Class, string Title, CMDShow nCmdShow)
        {
            this.hWnd = hWnd;
            this.Class = Class;
            this.Title = Title;
            this.nCmdShow = nCmdShow;
            Continue = true;
        }

        public CMDShow nCmdShow;
        public bool Continue;
    }
}