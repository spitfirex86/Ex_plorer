using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ex_plorer
{
    internal static class NativeIcon
    {
        internal static Icon GetSmallIcon(string path) => GetIcon(path, SHGFI.ICON | SHGFI.SMALLICON);

        internal static Icon GetLargeIcon(string path) => GetIcon(path, SHGFI.ICON | SHGFI.LARGEICON);

        private static Icon GetIcon(string path, SHGFI flags)
        {
            SHFILEINFO info = new SHFILEINFO();
            SHGetFileInfo(path, 0, ref info, (uint)Marshal.SizeOf(info), flags | SHGFI.USEFILEATTRIBUTES);
            return Icon.FromHandle(info.hIcon);
        }

        internal static string GetIconsAndTypeName(string path, out Icon smallIcon, out Icon largeIcon)
        {
            SHFILEINFO info = new SHFILEINFO();
            SHGetFileInfo(path, 0x80, ref info, (uint) Marshal.SizeOf(info), SHGFI.ICON | SHGFI.SMALLICON | SHGFI.USEFILEATTRIBUTES);
            smallIcon = Icon.FromHandle(info.hIcon);
            SHGetFileInfo(path, 0x80, ref info, (uint) Marshal.SizeOf(info), SHGFI.ICON | SHGFI.LARGEICON | SHGFI.TYPENAME | SHGFI.USEFILEATTRIBUTES);
            largeIcon = Icon.FromHandle(info.hIcon);
            return info.szTypeName;
        }

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO sfi, uint cbFileInfo, SHGFI uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

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

        [Flags]
        private enum SHGFI : uint
        {
            ICON = 0x100,
            DISPLAYNAME = 0x200,
            TYPENAME = 0x400,
            ATTRIBUTES = 0x800,
            ICONLOCATION = 0x1000,
            EXETYPE = 0x2000,
            SYSICONINDEX = 0x4000,
            LINKOVERLAY = 0x8000,
            SELECTED = 0x10000,
            ATTR_SPECIFIED = 0x20000,
            LARGEICON = 0x0,
            SMALLICON = 0x1,
            OPENICON = 0x2,
            SHELLICONSIZE = 0x4,
            PIDL = 0x8,
            USEFILEATTRIBUTES = 0x10,
            ADDOVERLAYS = 0x20,
            OVERLAYINDEX = 0x40
        }
    }
}