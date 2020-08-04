using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        private DirManager Manager { get; }

        private StatusBar statusBar;
        private StatusBarPanel itemsCount;
        private MenuItem[] viewModeItems;

        private List<MenuItem> selectionDependentItems = new List<MenuItem>();
        private List<MenuItem> clipboardDependentItems = new List<MenuItem>();


        public ExplorerForm(string path, bool showStatusBar = true, View viewMode = View.LargeIcon)
        {
            InitializeComponent();
            SetUpUI(showStatusBar, viewMode);

            Icon = Resources.dir;
            Text = path;

            Manager = new DirManager(path);
            folderView.LargeImageList = Manager.LargeIcons;
            folderView.SmallImageList = Manager.SmallIcons;

            //TODO: Clipboard
            //TODO: Drag and drop

            GetAllFiles();
        }

        private async void GetAllFiles()
        {
            itemsCount.Text = "Please wait...";
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
                if (!drive.IsReady) continue;

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

        private void SetUpUI(bool showStatusBar, View viewMode)
        {
            itemsCount = new StatusBarPanel();
            statusBar = new StatusBar
            {
                Dock = DockStyle.Bottom,
                Panels = {itemsCount},
                ShowPanels = true,
                SizingGrip = true,
                Visible = showStatusBar
            };
            Controls.Add(statusBar);
            folderView.View = viewMode;

            MenuItem[] goItems = {
                new MenuItem("-"),
                new MenuItem("&Copy Path", (sender, e) => Clipboard.SetText(Text)),
                new MenuItem("&Go To...", GoToPrompt, Shortcut.CtrlG),
                new MenuItem("&Up One Level", UpOneLevel, Shortcut.CtrlU),
            };
            MenuItem goMenu = new MenuItem("&Go", GetDrivesMenu());
            goMenu.MenuItems.AddRange(goItems);

            MenuItem viewMenu = new MenuItem("&View", new[]
            {
                new MenuItem("&Status Bar", ToggleStatusBar) { Checked = showStatusBar },
                new MenuItem("-"),
                new MenuItem("&Refresh", RefreshWindow, Shortcut.CtrlR), 
                new MenuItem("-"),
            });
            viewModeItems = new[] {
                new MenuItem("Large icons", ToggleFolderViewMode(View.LargeIcon))
                    { RadioCheck = true, Checked = folderView.View == View.LargeIcon },
                new MenuItem("List", ToggleFolderViewMode(View.List))
                    { RadioCheck = true, Checked = folderView.View == View.List },
                new MenuItem("Details", ToggleFolderViewMode(View.Details))
                    { RadioCheck = true, Checked = folderView.View == View.Details },
            };
            viewMenu.MenuItems.AddRange(viewModeItems);

            MenuItem copyItem = new MenuItem("&Copy", TriggerCopy, Shortcut.CtrlC);
            selectionDependentItems.Add(copyItem);
            MenuItem pasteItem = new MenuItem("&Paste", TriggerPaste, Shortcut.CtrlV);
            clipboardDependentItems.Add(pasteItem);

            MenuItem editMenu = new MenuItem("&Edit", new[]
            {
                copyItem,
                pasteItem,
                new MenuItem("-"),
                new MenuItem("Select &All", SelectAll, Shortcut.CtrlA)
            });
            editMenu.Popup += UpdateSelectionDependentMenu;
            editMenu.Popup += UpdateClipboardDependentMenu;

            MenuItem deleteItem = new MenuItem("&Delete", TriggerDelete, Shortcut.Del);
            selectionDependentItems.Add(deleteItem);
            MenuItem renameItem = new MenuItem("&Rename", TriggerRename, Shortcut.F2);
            selectionDependentItems.Add(renameItem);

            MenuItem fileMenu = new MenuItem("&File", new[]
            {
                new MenuItem("&New Folder", TriggerNewFolder),
                new MenuItem("-"),
                deleteItem,
                renameItem,
                new MenuItem("-"),
                new MenuItem("&Close", (sender, e) => Close()),
            });
            fileMenu.Popup += UpdateSelectionDependentMenu;

            //TODO: Main menu
            Menu = new MainMenu(new[]
            {
                fileMenu,
                editMenu,
                viewMenu,
                goMenu,
            });
        }

        private void NewWindow(string path)
        {
            ExplorerForm form = new ExplorerForm(path, statusBar.Visible, folderView.View);
            form.Show();
        }

        private EventHandler ToggleFolderViewMode(View view)
        {
            return (sender, e) =>
            {
                folderView.View = view;
                foreach (MenuItem item in viewModeItems)
                {
                    item.Checked = false;
                }

                ((MenuItem) sender).Checked = true;
            };
        }

        private void SelectAll(object sender, EventArgs e)
        {
            foreach (ListViewItem item in folderView.Items)
            {
                item.Selected = true;
            }
        }

        private void TriggerNewFolder(object sender, EventArgs e)
        {
            folderView.SelectedItems.Clear();

            string baseName = "New folder";
            string dirPath = Path.Combine(Manager.CurrentDir.FullName, baseName);

            int counter = 0;
            while (Directory.Exists(dirPath))
            {
                counter++;
                dirPath = Path.Combine(Manager.CurrentDir.FullName, baseName + $" ({counter})");
            }

            DirectoryInfo dir = new DirectoryInfo(dirPath);
            dir.Create();

            ListViewItem newItem = Manager.GetDirItem(dir);
            folderView.Items.Add(newItem);
            newItem.Selected = true;
            newItem.BeginEdit();
        }

        private void TriggerCopy(object sender, EventArgs e)
        {
            if (folderView.SelectedItems.Count == 0) return;

            StringCollection fileNames = new StringCollection();
            foreach (ListViewItem item in folderView.SelectedItems)
            {
                FileSystemInfo info = (FileSystemInfo)item.Tag;
                fileNames.Add(info.FullName);
            }

            Clipboard.SetFileDropList(fileNames);
        }

        private void TriggerPaste(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsFileDropList()) return;

            folderView.SelectedItems.Clear();

            StringCollection fileNames = Clipboard.GetFileDropList();
            foreach (string path in fileNames)
            {
                if (File.Exists(path))
                {
                    FileInfo source = new FileInfo(path);
                    string targetPath = Path.Combine(Manager.CurrentDir.FullName, source.Name);
                    source.CopyTo(targetPath, false);

                    FileInfo target = new FileInfo(targetPath);
                    ListViewItem newItem = Manager.GetFileItem(target);
                    folderView.Items.Add(newItem);
                    newItem.Selected = true;
                }
                else if (Directory.Exists(path))
                {
                    //TODO: recursive directory pasting
                    throw new NotImplementedException();
                }
            }
        }

        private void TriggerDelete(object sender, EventArgs e)
        {
            int count = folderView.SelectedItems.Count;
            if (count == 0) return;

            string message = count > 1 ? $"these {count} files" : folderView.SelectedItems[0].Text;
            DialogResult result = MessageBox.Show($"Are you sure you want to delete {message}?", "Delete File", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            foreach (ListViewItem item in folderView.SelectedItems)
            {
                FileSystemInfo info = (FileSystemInfo)item.Tag;
                if (info is FileInfo file)
                {
                    file.Delete();
                }
                else if (info is DirectoryInfo dir)
                {
                    dir.Delete(true);
                }

                folderView.Items.Remove(item);
            }
        }

        private void TriggerRename(object sender, EventArgs e)
        {
            if (folderView.SelectedItems.Count == 0) return;

            ListViewItem item = folderView.SelectedItems[0];
            item.BeginEdit();
        }

        private void UpdateSelectionDependentMenu(object sender, EventArgs e)
        {
            bool enabled = folderView.SelectedItems.Count > 0;
            foreach (MenuItem item in selectionDependentItems)
            {
                item.Enabled = enabled;
            }
        }

        private void UpdateClipboardDependentMenu(object sender, EventArgs e)
        {
            bool enabled = Clipboard.ContainsFileDropList();
            foreach (MenuItem item in clipboardDependentItems)
            {
                item.Enabled = enabled;
            }
        }

        private void RefreshWindow(object sender, EventArgs e)
        {
            folderView.Items.Clear();
            GetAllFiles();
        }

        private void UpOneLevel(object sender, EventArgs e)
        {
            if (Manager.CurrentDir.Parent == null) return;
            NewWindow(Manager.CurrentDir.Parent.FullName);
        }

        private void GoToDirFromMenu(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            string path = (string)item.Tag;
            NewWindow(path);
        }

        private void GoToPrompt(object sender, EventArgs e)
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

        private void ToggleStatusBar(object sender, EventArgs e)
        {
            if (statusBar.Visible) statusBar.Hide();
            else statusBar.Show();
            ((MenuItem)sender).Checked = statusBar.Visible;
        }

        private void folderView_ItemActivate(object sender, EventArgs e)
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

        private void folderView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            ListViewItem item = folderView.Items[e.Item];
            string newName = e.Label;
            if (string.IsNullOrWhiteSpace(newName))
            {
                e.CancelEdit = true;
                return;
            }

            FileSystemInfo info = (FileSystemInfo)item.Tag;

            if (info is FileInfo file)
            {
                file.MoveTo(Path.Combine(Manager.CurrentDir.FullName, newName));
                item.ImageKey = Manager.GetIconKey(file);
            }
            else if (info is DirectoryInfo dir)
            {
                dir.MoveTo(Path.Combine(Manager.CurrentDir.FullName, newName));
            }
        }

        private void folderView_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool enabled = folderView.SelectedItems.Count > 0;

            foreach (MenuItem item in selectionDependentItems)
            {
                item.Enabled = enabled;
            }
        }
    }
}
