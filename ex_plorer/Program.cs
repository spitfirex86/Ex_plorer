using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace ex_plorer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.VisualStyleState = VisualStyleState.NoneEnabled;
            Application.SetCompatibleTextRenderingDefault(false);

            string path = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();

            ExplorerForm form = new ExplorerForm(path);
            form.Show();

            Application.Run();
        }
    }
}
