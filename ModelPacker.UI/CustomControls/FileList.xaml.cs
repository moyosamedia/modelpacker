using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace ModelPacker.UI.CustomControls
{
    public partial class FileList : UserControl
    {
        public string[] files
        {
            get
            {
                ItemCollection items = FilesList.Items;
                string[] retVal = new string[items.Count];
                for (int i = 0; i < items.Count; i++)
                {
                    retVal[i] = items[i].ToString();
                }

                return retVal;
            }
        }

        public FileList()
        {
            InitializeComponent();
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            string lastAddedFile = FilesList.Items.Count > 0
                ? FilesList.Items[FilesList.Items.Count - 1].ToString()
                : null;

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = !string.IsNullOrEmpty(lastAddedFile)
                    ? new FileInfo(lastAddedFile).DirectoryName
                    : System.Reflection.Assembly.GetEntryAssembly().Location,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string fileName in openFileDialog.FileNames)
                {
                    Add(fileName);
                }
            }
        }

        public void Add(string file)
        {
            // TODO: Sort FilesList based on file name
            if (!ContainsFile(file))
            {
                FilesList.Items.Add(file);
            }
        }

        private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (FilesList.SelectedIndex > -1)
            {
                while (FilesList.SelectedItems.Count > 0)
                {
                    FilesList.Items.RemoveAt(FilesList.SelectedIndex);
                }
            }
        }

        private void ClearButton_OnClick(object sender, RoutedEventArgs e)
        {
            FilesList.Items.Clear();
        }

        public bool ContainsFile(string file)
        {
            if (string.IsNullOrEmpty(file))
                return false;

            foreach (object item in FilesList.Items)
            {
                if (item as string == file)
                {
                    return true;
                }
            }

            return false;
        }
    }
}