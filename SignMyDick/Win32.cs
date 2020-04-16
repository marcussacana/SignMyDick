using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SignMyDick
{
    unsafe class Win32
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(void* hWnd, StringBuilder lpClassName, int nMaxCount);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(void* hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(void* hWnd);

        public static string GetWindowText(void* hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }


        public static string GetClassNameFromHandle(void* hWnd) {
            StringBuilder ClassName = new StringBuilder(100);
            GetClassName(hWnd, ClassName, ClassName.Capacity);
            return ClassName.ToString();
        }
    }
}
