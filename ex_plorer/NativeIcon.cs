using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ex_plorer
{
    internal static class NativeIcon
    {
        internal static Icon GetSmallIcon(string path) => GetIcon(path, SHGFI_ICON | SHGFI_SMALLICON);

        internal static Icon GetLargeIcon(string path) => GetIcon(path, SHGFI_ICON | SHGFI_LARGEICON);

        private static Icon GetIcon(string path, uint flags)
        {
            SHFILEINFO info = new SHFILEINFO();
            SHGetFileInfo(path, 0, ref info, (uint)Marshal.SizeOf(info), flags);
            return Icon.FromHandle(info.hIcon);
        }

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x1;

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO sfi, uint cbFileInfo, uint uFlags);

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            internal IntPtr hIcon;
            internal IntPtr iIcon;
            internal uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            internal string szTypeName;
        }
    }
}