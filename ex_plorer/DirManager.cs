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
        internal ImageList LargeIcons { get; } 
        internal ImageList SmallIcons { get; }

        internal DirManager(string path)
        {
            Path = path;
            CurrentDir = new DirectoryInfo(path);

            LargeIcons = new ImageList
            {
                ImageSize = new Size(32, 32),
                ColorDepth = ColorDepth.Depth32Bit,
                Images = {
                    { DirIcon, new Icon(Resources.dir, 32, 32) },
                    { FileIcon, new Icon(Resources.file, 32, 32) }
                }
            };

            SmallIcons = new ImageList
            {
                ImageSize = new Size(16, 16),
                ColorDepth = ColorDepth.Depth32Bit,
                Images = {
                    { DirIcon, new Icon(Resources.dir, 16, 16) },
                    { FileIcon, new Icon(Resources.file, 16, 16) }
                }
            };
        }

        internal IEnumerable<ListViewItem> GetAllFiles()
        {
            IEnumerable<ListViewItem> items = CurrentDir.EnumerateFileSystemInfos().Select(info =>
            {
                ListViewItem item = new ListViewItem(info.Name);
                item.Tag = info;
                bool isDirectory = false;

                if (info is FileInfo file)
                {
                    item.ImageKey = SetIcon(file);
                }
                else if (info is DirectoryInfo dir)
                {
                    isDirectory = true;
                    item.ImageKey = DirIcon;
                }

                return new { isDirectory, item };
            }).OrderByDescending(arg => arg.isDirectory).Select(arg => arg.item);

            return items;
        }

        private string SetIcon(FileInfo file)
        {
            string key;
            if (file.Extension == "")
            {
                key = FileIcon;
            }
            else if (file.Extension == ".exe")
            {
                key = file.Name;
                ExtractIcon(key, file.FullName);
            }
            else
            {
                key = file.Extension;
                if (!LargeIcons.Images.ContainsKey(file.Extension))
                {
                    ExtractIcon(key, file.FullName);
                }
            }

            return key;
        }

        private void ExtractIcon(string key, string path)
        {
            Icon largeIcon = NativeIcon.GetLargeIcon(path);
            Icon smallIcon = NativeIcon.GetSmallIcon(path);
            LargeIcons.Images.Add(key, largeIcon);
            SmallIcons.Images.Add(key, smallIcon);
        }
    }
}