﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace SignMyDick
{
    unsafe abstract class Wrapper
    {
        static bool WOW64 => !Environment.Is64BitProcess && Environment.Is64BitOperatingSystem;

        internal static string CurrentDll => Assembly.GetExecutingAssembly().Location;
        internal static string CurrentDllName => Path.GetFileName(CurrentDll).ToLower();
        internal static string CurrentDllPath => Path.GetDirectoryName(CurrentDll);

        internal static void* LoadLibrary(string lpFileName)
        {
            string DllPath = lpFileName;
            if (lpFileName.Length < 2 || lpFileName[1] != ':')
            {
                string DLL = Path.GetFileNameWithoutExtension(lpFileName);
                DllPath = Path.Combine(Environment.CurrentDirectory, $"{DLL}_ori.dll");


                if (!File.Exists(DllPath) && CurrentDllName != lpFileName.ToLower())
                    DllPath = Path.Combine(Environment.CurrentDirectory, $"{DLL}.dll");

                if (!File.Exists(DllPath) && CurrentDllName != lpFileName.ToLower())
                    DllPath = Path.Combine(CurrentDllPath, $"{DLL}_ori.dll");

                if (!File.Exists(DllPath) && CurrentDllName != lpFileName.ToLower())
                    DllPath = Path.Combine(CurrentDllPath, $"{DLL}.dll.ori");

                if (!File.Exists(DllPath))
                {
                    DllPath = WOW64 ? Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) : Environment.SystemDirectory;
                    DllPath = Path.Combine(DllPath, $"{DLL}.dll");
                }
            }

            void* Handler = LoadLibraryW(DllPath);

            if (Handler == null)
                Environment.Exit(0x505);//ERROR_DELAY_LOAD_FAILED


            return Handler;
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern void* LoadLibraryW(string lpFileName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        internal static extern void* GetProcAddress(void* hModule, string procName);

        internal static T GetDelegate<T>(void* Handler, string Function, bool Optional = true) where T : Delegate
        {
            var Address = GetProcAddress(Handler, Function);
            if (Address == null)
            {
                if (Optional)
                {
                    return null;
                }

                Environment.Exit(0x505);//ERROR_DELAY_LOAD_FAILED
            }
            return (T)Marshal.GetDelegateForFunctionPointer(new IntPtr(Address), typeof(T));
        }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr RET_28(IntPtr A, IntPtr B, IntPtr C, IntPtr D, IntPtr E, IntPtr F, IntPtr G, IntPtr H, IntPtr I, IntPtr J, IntPtr K, IntPtr L, IntPtr M, IntPtr N, IntPtr O, IntPtr P, IntPtr Q, IntPtr R, IntPtr S, IntPtr T, IntPtr U, IntPtr V, IntPtr W, IntPtr X, IntPtr Y, IntPtr Z, IntPtr AA, IntPtr AB);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr RET_12(IntPtr A, IntPtr B, IntPtr C, IntPtr D, IntPtr E, IntPtr F, IntPtr G, IntPtr H, IntPtr I, IntPtr J, IntPtr K, IntPtr L);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr RET_11(IntPtr A, IntPtr B, IntPtr C, IntPtr D, IntPtr E, IntPtr F, IntPtr G, IntPtr H, IntPtr I, IntPtr J, IntPtr K);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr RET_10(IntPtr A, IntPtr B, IntPtr C, IntPtr D, IntPtr E, IntPtr F, IntPtr G, IntPtr H, IntPtr I, IntPtr J);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr RET_9(IntPtr A, IntPtr B, IntPtr C, IntPtr D, IntPtr E, IntPtr F, IntPtr G, IntPtr H, IntPtr I);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr RET_8(IntPtr A, IntPtr B, IntPtr C, IntPtr D, IntPtr E, IntPtr F, IntPtr G, IntPtr H);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr RET_7(IntPtr A, IntPtr B, IntPtr C, IntPtr D, IntPtr E, IntPtr F, IntPtr G);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr RET_6(IntPtr A, IntPtr B, IntPtr C, IntPtr D, IntPtr E, IntPtr F);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr RET_5(IntPtr A, IntPtr B, IntPtr C, IntPtr D, IntPtr E);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr RET_4(IntPtr A, IntPtr B, IntPtr C, IntPtr D);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr RET_3(IntPtr A, IntPtr B, IntPtr C);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr RET_2(IntPtr A, IntPtr B);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr RET_1(IntPtr A);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr RET_0();


        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void NULL_12(IntPtr A, IntPtr B, IntPtr C, IntPtr D, IntPtr E, IntPtr F, IntPtr G, IntPtr H, IntPtr I, IntPtr J, IntPtr K, IntPtr L);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void NULL_8(IntPtr A, IntPtr B, IntPtr C, IntPtr D, IntPtr E, IntPtr F, IntPtr G, IntPtr H);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void NULL_6(IntPtr A, IntPtr B, IntPtr C, IntPtr D, IntPtr E, IntPtr F);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void NULL_5(IntPtr A, IntPtr B, IntPtr C, IntPtr D, IntPtr E);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void NULL_4(IntPtr A, IntPtr B, IntPtr C, IntPtr D);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void NULL_3(IntPtr A, IntPtr B, IntPtr C);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void NULL_2(IntPtr A, IntPtr B);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void NULL_1(IntPtr A);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void NULL_0();
    }
}
