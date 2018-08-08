using System.IO;
using System.Windows;
using System.Windows.Forms;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using UserControl = System.Windows.Controls.UserControl;

namespace ModelPacker.UI.CustomControls
{
    public partial class FilePath : UserControl
    {
        public bool SelectDirectory { get; set; }

        public string FullPath
        {
            get => Path.Text;
            set => Path.Text = value;
        }

        public FilePath()
        {
            InitializeComponent();
        }

        private void BrowseButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectDirectory)
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
                {
                    SelectedPath = string.IsNullOrEmpty(FullPath)
                        ? System.Reflection.Assembly.GetEntryAssembly().Location
                        : FullPath,
                    ShowNewFolderButton = true
                };
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    Path.Text = folderBrowserDialog.SelectedPath;
                }
            }
            else
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Multiselect = false,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    InitialDirectory = string.IsNullOrEmpty(FullPath)
                        ? System.Reflection.Assembly.GetEntryAssembly().Location
                        : new FileInfo(FullPath).DirectoryName,
                    RestoreDirectory = true,
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    Path.Text = openFileDialog.FileName;
                }
            }
        }
    }
}