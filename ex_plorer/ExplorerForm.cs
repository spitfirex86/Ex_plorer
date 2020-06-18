using ex_plorer.Properties;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ex_plorer
{
    public partial class ExplorerForm : Form
    {
        private string Path { get; }
        private DirectoryInfo CurrentDir { get; }

        private ImageList icons;
        private StatusBar statusBar;
        private StatusBarPanel itemsCount = new StatusBarPanel();

        public ExplorerForm(string path)
        {
            InitializeComponent();
            SetUpMenus();

            Path = path;
            CurrentDir = new DirectoryInfo(path);

            Icon = Resources.dir;
            Text = path;

            folderView.LargeImageList = icons = new ImageList
            {
                ImageSize = new Size(32, 32),
                ColorDepth = ColorDepth.Depth32Bit,
            };
            icons.Images.Add("$dir", Resources.dir);
            icons.Images.Add("$file", Resources.file);
        }

        private void GetAllFiles()
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
                    item.ImageKey = "$dir";
                }

                return new { isDirectory, item };
            }).OrderByDescending(arg => arg.isDirectory).Select(arg => arg.item);

            folderView.Items.AddRange(items.ToArray());
        }

        private string SetIcon(FileInfo file)
        {
            string key;
            if (file.Extension == "")
            {
                key = "$file";
            }
            else if (file.Extension == ".exe")
            {
                key = file.Name;
                Icon fileIcon = Icon.ExtractAssociatedIcon(file.FullName) ?? SystemIcons.Application;
                icons.Images.Add(file.Name, fileIcon);
            }
            else
            {
                key = file.Extension;

                if (!icons.Images.ContainsKey(file.Extension))
                {
                    Icon fileIcon = Icon.ExtractAssociatedIcon(file.FullName) ?? SystemIcons.WinLogo;
                    icons.Images.Add(file.Extension, fileIcon);
                }
            }

            return key;
        }

        private void SetUpMenus()
        {
            statusBar = new StatusBar
            {
                Dock = DockStyle.Bottom,
                Panels = { itemsCount },
                ShowPanels = true, 
                SizingGrip = true,
            };
            Controls.Add(statusBar);

            MenuItem toggleStatusBar = new MenuItem("&Status Bar") {Checked = statusBar.Visible};
            toggleStatusBar.Click += (sender, args) =>
            {
                if (statusBar.Visible) statusBar.Hide();
                else statusBar.Show();
                toggleStatusBar.Checked = statusBar.Visible;
            };
            MenuItem upOneLevel = new MenuItem("&Up One Level", (sender, args) =>
            {
                if (CurrentDir.Parent == null) return;
                ExplorerForm form = new ExplorerForm(CurrentDir.Parent.FullName);
                form.Show();
            });

            Menu = new MainMenu(new[]
            {
                new MenuItem("&File"),
                new MenuItem("&Edit"),
                new MenuItem("&View", new []
                {
                    toggleStatusBar,
                    new MenuItem("-"),
                    upOneLevel
                }),
                new MenuItem("&Help"),
            });
        }

        private void folderView_ItemActivate(object sender, System.EventArgs e)
        {
            if (folderView.SelectedItems.Count == 0) return;

            ListViewItem item = folderView.SelectedItems[0];
            FileSystemInfo info = (FileSystemInfo)item.Tag;

            if (info is FileInfo file)
            {
                Process.Start(file.FullName);
            }
            else if (info is DirectoryInfo dir)
            {
                ExplorerForm form = new ExplorerForm(dir.FullName);
                form.Show();
            }
        }

        private void ExplorerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Application.OpenForms.Count == 0)
                Application.Exit();
        }

        private async void ExplorerForm_Load(object sender, System.EventArgs e)
        {
            itemsCount.Text = "Please wait...";
            await Task.Run(GetAllFiles);
            itemsCount.Text = $"{folderView.Items.Count} object(s)";
        }
    }
}
