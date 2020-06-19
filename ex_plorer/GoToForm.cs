using System.IO;
using System.Windows.Forms;

namespace ex_plorer
{
    public partial class GotoForm : Form
    {
        public string Result { get; private set; }

        public GotoForm()
        {
            InitializeComponent();
        }

        private void Go_Click(object sender, System.EventArgs e)
        {
            string path = PathBox.Text;
            if (Path.IsPathRooted(path) && Directory.Exists(path))
            {
                Result = path;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
