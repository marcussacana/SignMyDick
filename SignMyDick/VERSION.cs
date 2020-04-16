﻿using System;
using System.Runtime.InteropServices;
using static SignMyDick.Wrapper;

namespace SignMyDick
{
    /// <summary>
    /// This is a wrapper to the VERSION.dll
    /// </summary>
    public unsafe static class VERSION
    {
        static void* RealHandler = null;
        static VERSION()
        {
            if (RealHandler != null)
                return;

            RealHandler = LoadLibrary("VERSION.dll");

            if (RealHandler == null)
                Environment.Exit(0x505);//ERROR_DELAY_LOAD_FAILED

            dGetFileVersionInfoSizeExW = GetDelegate<RET_3>(RealHandler, "GetFileVersionInfoSizeExW", false);
            dGetFileVersionInfoSizeW = GetDelegate<RET_2>(RealHandler, "GetFileVersionInfoSizeW", false);
            dGetFileVersionInfoW = GetDelegate<RET_4>(RealHandler, "GetFileVersionInfoW", false);
            dGetFileVersionInfoExW = GetDelegate<RET_5>(RealHandler, "GetFileVersionInfoExW", false);
            dVerQueryValueW = GetDelegate<RET_4>(RealHandler, "VerQueryValueW", false);
            dVerQueryValueA = GetDelegate<RET_4>(RealHandler, "VerQueryValueA", false);
            dGetFileVersionInfoSizeA = GetDelegate<RET_2>(RealHandler, "GetFileVersionInfoSizeA", false);
            dGetFileVersionInfoA = GetDelegate<RET_4>(RealHandler, "GetFileVersionInfoA", false);
            dGetFileVersionInfoByHandle = GetDelegate<RET_4>(RealHandler, "GetFileVersionInfoByHandle", false);
            dVerFindFileA = GetDelegate<RET_8>(RealHandler, "VerFindFileA", false);
            dVerInstallFileA = GetDelegate<RET_8>(RealHandler, "VerInstallFileA", false);
            dVerFindFileW = GetDelegate<RET_8>(RealHandler, "VerFindFileW", false);
            dVerInstallFileW = GetDelegate<RET_8>(RealHandler, "VerInstallFileW", false);

            Engine.Initialize();

        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static IntPtr GetFileVersionInfoSizeExW(IntPtr dwFlags, IntPtr lpwstrFilename, IntPtr lpdwHandle)
        {
            return dGetFileVersionInfoSizeExW(dwFlags, lpwstrFilename, lpdwHandle);
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static IntPtr GetFileVersionInfoSizeW(IntPtr lptstrFilename, IntPtr lpdwHandle)
        {
            return dGetFileVersionInfoSizeW(lptstrFilename, lpdwHandle);
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static IntPtr GetFileVersionInfoW(IntPtr lptstrFilename, IntPtr dwHandle, IntPtr dwLen, IntPtr lpData)
        {
            return dGetFileVersionInfoW(lptstrFilename, dwHandle, dwLen, lpData);
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static IntPtr GetFileVersionInfoExW(IntPtr dwFlags, IntPtr lpwstrFilename, IntPtr dwHandle, IntPtr dwLen, IntPtr lpData)
        {
            return dGetFileVersionInfoExW(dwFlags, lpwstrFilename, dwHandle, dwLen, lpData);
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static IntPtr VerQueryValueW(IntPtr pBlock, IntPtr lpSubBlock, IntPtr lplpBuffer, IntPtr puLen)
        {
            return dVerQueryValueW(pBlock, lpSubBlock, lplpBuffer, puLen);
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static IntPtr VerQueryValueA(IntPtr pBlock, IntPtr lpSubBlock, IntPtr lplpBuffer, IntPtr puLen)
        {
            return dVerQueryValueA(pBlock, lpSubBlock, lplpBuffer, puLen);
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static IntPtr GetFileVersionInfoSizeA(IntPtr lptstrFilename, IntPtr lpdwHandle)
        {
            return dGetFileVersionInfoSizeA(lptstrFilename, lpdwHandle);
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static IntPtr GetFileVersionInfoA(IntPtr lptstrFilename, IntPtr dwHandle, IntPtr dwLen, IntPtr lpData)
        {
            return dGetFileVersionInfoA(lptstrFilename, dwHandle, dwLen, lpData);
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static IntPtr GetFileVersionInfoByHandle(IntPtr Src, IntPtr hFile, IntPtr a3, IntPtr a4)
        {
            return dGetFileVersionInfoByHandle(Src, hFile, a3, a4);
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static IntPtr VerFindFileA(IntPtr uFlags, IntPtr szFileName, IntPtr szWinDir, IntPtr szAppDir, IntPtr szCurDir, IntPtr lpuCurDirLen, IntPtr szDestDir, IntPtr lpuDestDirLen)
        {
            return dVerFindFileA(uFlags, szFileName, szWinDir, szAppDir, szCurDir, lpuCurDirLen, szDestDir, lpuDestDirLen);
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static IntPtr VerInstallFileA(IntPtr uFlags, IntPtr szSrcFileName, IntPtr szDestFileName, IntPtr szSrcDir, IntPtr szDestDir, IntPtr szCurDir, IntPtr szTmpFile, IntPtr lpuTmpFileLen)
        {
            return dVerInstallFileA(uFlags, szSrcFileName, szDestFileName, szSrcDir, szDestDir, szCurDir, szTmpFile, lpuTmpFileLen);
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static IntPtr VerFindFileW(IntPtr uFlags, IntPtr szFileName, IntPtr szWinDir, IntPtr szAppDir, IntPtr szCurDir, IntPtr lpuCurDirLen, IntPtr szDestDir, IntPtr lpuDestDirLen)
        {
            return dVerFindFileW(uFlags, szFileName, szWinDir, szAppDir, szCurDir, lpuCurDirLen, szDestDir, lpuDestDirLen);
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static IntPtr VerInstallFileW(IntPtr uFlags, IntPtr szSrcFileName, IntPtr szDestFileName, IntPtr szSrcDir, IntPtr szDestDir, IntPtr szCurDir, IntPtr szTmpFile, IntPtr lpuTmpFileLen)
        {
            return dVerInstallFileW(uFlags, szSrcFileName, szDestFileName, szSrcDir, szDestDir, szCurDir, szTmpFile, lpuTmpFileLen);
        }


        static RET_3 dGetFileVersionInfoSizeExW;
        static RET_2 dGetFileVersionInfoSizeW;
        static RET_4 dGetFileVersionInfoW;
        static RET_5 dGetFileVersionInfoExW;
        static RET_4 dVerQueryValueW;
        static RET_4 dVerQueryValueA;
        static RET_2 dGetFileVersionInfoSizeA;
        static RET_4 dGetFileVersionInfoA;
        static RET_4 dGetFileVersionInfoByHandle;
        static RET_8 dVerFindFileA;
        static RET_8 dVerInstallFileA;
        static RET_8 dVerFindFileW;
        static RET_8 dVerInstallFileW;

    }
}