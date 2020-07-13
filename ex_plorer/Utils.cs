namespace ex_plorer
{
    public static class Utils
    {
        private const long EB = 0x1000000000000000;
        private const long PB = 0x4000000000000;
        private const long TB = 0x10000000000;
        private const long GB = 0x40000000;
        private const long MB = 0x100000;
        private const long KB = 0x400;

        public static string ReadableFileSize(this long val)
        {
            long absoluteVal = (val < 0 ? -val : val);
            string suffix;
            double readable;

            if (absoluteVal >= EB)
            {
                suffix = "EB";
                readable = (val >> 50);
            }
            else if (absoluteVal >= PB)
            {
                suffix = "PB";
                readable = (val >> 40);
            }
            else if (absoluteVal >= TB)
            {
                suffix = "TB";
                readable = (val >> 30);
            }
            else if (absoluteVal >= GB)
            {
                suffix = "GB";
                readable = (val >> 20);
            }
            else if (absoluteVal >= MB)
            {
                suffix = "MB";
                readable = (val >> 10);
            }
            else if (absoluteVal >= KB)
            {
                suffix = "KB";
                readable = val;
            }
            else
            {
                return $"{val:0} B";
            }

            readable /= 1024.0;

            return $"{readable:0.###} {suffix}";
        }

        public static string FileSizeInKB(this long val)
        {
            // This should result in the same rounding as in Microsoft's Explorer.
            // Value is n if the size is exactly 1024n KB, or n+1 if it's at least 1 byte over (rounded up).
            long readable = (val + 1023) / 1024;
            return $"{readable:N0} KB";
        }
    }
}