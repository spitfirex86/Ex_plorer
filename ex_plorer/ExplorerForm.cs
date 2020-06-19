using ex_plorer.Properties;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ex_plorer
{
    public partial class ExplorerForm : Form
    {
        private const bool StatusBarVisibleOnStart = true;

        private DirManager Manager { get; }

        private StatusBar statusBar;
        private StatusBarPanel itemsCount;
        private MenuItem[] viewModeItems;

        public ExplorerForm(string path)
        {
            InitializeComponent();
            SetUpMenus();

            Icon = Resources.dir;
            Text = path;

            Manager = new DirManager(path);
            folderView.LargeImageList = Manager.LargeIcons;
            folderView.SmallImageList = Manager.SmallIcons;

            //TODO: Clipboard
            //TODO: Drag and drop
        }

        private async void GetAllFiles()
        {
            ListViewItem[] items = await Task.Run(Manager.GetAllFiles().ToArray);
            folderView.Items.AddRange(items);
            itemsCount.Text = $"{folderView.Items.Count} object(s)";
        }

        private MenuItem[] GetDrivesMenu()
        {
            MenuItem[] items = new MenuItem[DirManager.Drives.Length];
            for (int i = 0; i < DirManager.Drives.Length; i++)
            {
                DriveInfo drive = DirManager.Drives[i];
                string label = drive.VolumeLabel;
                if (string.IsNullOrEmpty(label))
                {
                    switch (drive.DriveType)
                    {
                        case DriveType.Removable:
                            label = "Removable Disk";
                            break;
                        case DriveType.Fixed:
                            label = "Local Disk";
                            break;
                        case DriveType.Network:
                            label = "Network Drive";
                            break;
                        case DriveType.CDRom:
                            label = "CD-ROM Drive";
                            break;
                    }
                }
                MenuItem item = new MenuItem($"{label} ({drive.Name})", GoToDirFromMenu);
                item.Tag = drive.Name;

                items[i] = item;
            }
            return items;
        }

        private void SetUpMenus()
        {
            itemsCount = new StatusBarPanel();
            statusBar = new StatusBar
            {
                Dock = DockStyle.Bottom,
                Panels = { itemsCount },
                ShowPanels = true,
                SizingGrip = true,
                Visible = StatusBarVisibleOnStart
            };
            Controls.Add(statusBar);

            MenuItem goMenu = new MenuItem("&Go", GetDrivesMenu());
            MenuItem[] goItems = {
                new MenuItem("-"),
                new MenuItem("&Go To...", GoToPrompt),
                new MenuItem("&Up One Level", UpOneLevel),
            };
            goMenu.MenuItems.AddRange(goItems);

            MenuItem viewMenu = new MenuItem("&View", new[]
            {
                new MenuItem("&Status Bar", ToggleStatusBar) { Checked = StatusBarVisibleOnStart },
                new MenuItem("-"),
            });
            viewModeItems = new[] {
                new MenuItem("Large icons", ToggleFolderViewMode(View.LargeIcon))
                    { RadioCheck = true, Checked = folderView.View == View.LargeIcon },
                new MenuItem("List", ToggleFolderViewMode(View.List))
                    { RadioCheck = true, Checked = folderView.View == View.List },
            };
            viewMenu.MenuItems.AddRange(viewModeItems);

            //TODO: Main menu
            Menu = new MainMenu(new[]
            {
                new MenuItem("&File", new[]
                {
                    new MenuItem("Not yet implemented"),
                    new MenuItem("-"),
                    new MenuItem("&Close", (sender, e) => Close()),
                }),
                new MenuItem("&Edit", new[]
                {
                    new MenuItem("Not yet implemented"),
                }),
                viewMenu,
                goMenu,
            });
        }

        private void NewWindow(string path)
        {
            ExplorerForm form = new ExplorerForm(path);
            form.Show();
        }

        private void UpOneLevel(object sender, System.EventArgs e)
        {
            if (Manager.CurrentDir.Parent == null) return;
            NewWindow(Manager.CurrentDir.Parent.FullName);
        }

        private void GoToDirFromMenu(object sender, System.EventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            string path = (string)item.Tag;
            NewWindow(path);
        }

        private void GoToPrompt(object sender, System.EventArgs e)
        {
            GotoForm goTo = new GotoForm();
            goTo.ShowDialog();

            if (goTo.DialogResult != DialogResult.OK) return;
            if (string.IsNullOrEmpty(goTo.Result))
            {
                MessageBox.Show("Invalid path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            NewWindow(goTo.Result);
        }

        private void ToggleStatusBar(object sender, System.EventArgs e)
        {
            if (statusBar.Visible) statusBar.Hide();
            else statusBar.Show();
            ((MenuItem)sender).Checked = statusBar.Visible;
        }

        private System.EventHandler ToggleFolderViewMode(View view)
        {
            return (sender, e) =>
            {
                folderView.View = view;
                foreach (MenuItem item in viewModeItems)
                {
                    item.Checked = false;
                }
                ((MenuItem)sender).Checked = true;
            };
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
                NewWindow(dir.FullName);
            }
        }

        private void ExplorerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Application.OpenForms.Count == 0)
                Application.Exit();
        }

        private void ExplorerForm_Load(object sender, System.EventArgs e)
        {
            itemsCount.Text = "Please wait...";
            GetAllFiles();
        }
    }
}
