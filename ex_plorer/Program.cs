using System;
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
        static void Main()
        {
            //Application.EnableVisualStyles();
            Application.VisualStyleState = VisualStyleState.NoneEnabled;
            Application.SetCompatibleTextRenderingDefault(false);

            ExplorerForm form = new ExplorerForm(@"C:\");
            form.Show();

            Application.Run();
        }
    }
}
