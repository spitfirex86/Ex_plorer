using ex_plorer.Properties;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ex_plorer
{
    internal class DirManager
    {
        private const string DirIcon = "$dir";
        private const string FileIcon = "$file";

        internal static DriveInfo[] Drives { get; } = DriveInfo.GetDrives();

        private string Path { get; }
        internal DirectoryInfo CurrentDir { get; }

        internal static Dictionary<string, IconPair> IconDictionary { get; }

        internal ImageList LargeIcons { get; }
        internal ImageList SmallIcons { get; }

        internal List<string> IconsSet { get; }

        static DirManager()
        {
            IconDictionary = new Dictionary<string, IconPair>();
        }

        internal DirManager(string path)
        {
            Path = path;
            CurrentDir = new DirectoryInfo(path);

            IconsSet = new List<string>();

            LargeIcons = new ImageList
            {
                ImageSize = new Size(32, 32),
                ColorDepth = ColorDepth.Depth32Bit,
                Images =
                {
                    { DirIcon, new Icon(Resources.dir, 32, 32) },
                    { FileIcon, new Icon(Resources.file, 32, 32) }
                }
            };

            SmallIcons = new ImageList
            {
                ImageSize = new Size(16, 16),
                ColorDepth = ColorDepth.Depth32Bit,
                Images =
                {
                    { DirIcon, new Icon(Resources.dir, 16, 16) },
                    { FileIcon, new Icon(Resources.file, 16, 16) }
                }
            };
        }

        //TODO: This is slow, use Win32 function calls instead of DirectoryInfo
        internal IEnumerable<ListViewItem> GetAllFiles()
        {
            IEnumerable<ListViewItem> items = CurrentDir.EnumerateFileSystemInfos().Select(info =>
            {
                ListViewItem item = null;
                bool isDirectory = false;

                if (info is FileInfo file)
                {
                    item = GetFileItem(file);

                }
                else if (info is DirectoryInfo dir)
                {
                    item = GetDirItem(dir);
                    isDirectory = true;
                }

                return new { isDirectory, item };
            }).OrderByDescending(arg => arg.isDirectory).Select(arg => arg.item);

            return items;
        }

        internal ListViewItem GetFileItem(FileInfo file)
        {
            ListViewItem item = new ListViewItem(file.Name);
            item.SubItems.AddRange(new[]
            {
                file.Length.FileSizeInKB(),
                $"{file.Extension} File",
                file.LastWriteTime.ToString()
            });
            item.ImageKey = GetIconKey(file);
            item.Tag = file;

            return item;
        }

        internal ListViewItem GetDirItem(DirectoryInfo dir)
        {
            ListViewItem item = new ListViewItem(dir.Name);
            item.SubItems.AddRange(new[] {"", "Directory", dir.LastWriteTime.ToString()});
            item.ImageKey = DirIcon;
            item.Tag = dir;

            return item;
        }

        internal string GetIconKey(FileInfo file)
        {
            string key;
            string ext = file.Extension;
            if (file.Extension == "")
            {
                key = FileIcon;
            }
            else if (ext == ".exe" || ext == ".lnk" || ext == ".ico")
            {
                key = file.Name;
                ExtractIcon(key, file.FullName);
            }
            else
            {
                key = ext;
                if (!IconsSet.Contains(key))
                {
                    ExtractIcon(key, file.FullName);
                }
            }

            return key;
        }

        private void ExtractIcon(string key, string path)
        {
            IconsSet.Add(key);

            Icon smallIcon, largeIcon;

            if (IconDictionary.TryGetValue(key, out IconPair icons))
            {
                smallIcon = icons.Small;
                largeIcon = icons.Large;
            }
            else
            {
                //TODO: type names
                string typeName = NativeIcon.GetIconsAndTypeName(path, out smallIcon, out largeIcon);
                IconDictionary.Add(key, new IconPair(smallIcon, largeIcon));
            }

            LargeIcons.Images.Add(key, largeIcon);
            SmallIcons.Images.Add(key, smallIcon);
        }
    }

    internal struct IconPair
    {
        internal Icon Small;
        internal Icon Large;

        internal IconPair(Icon small, Icon large)
        {
            Small = small;
            Large = large;
        }
    }
}