using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ModelPacker.Logger;

namespace ModelPacker.UI.CustomControls
{
    public partial class FileList : UserControl
    {
        public List<string> files { get; private set; } = new List<string>();
        public bool sort = true;

        public Func<string, bool> predicate;

        public FileList()
        {
            InitializeComponent();
            FilesList.ItemsSource = files;
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

                RefreshList();
            }
        }

        public void Add(string file, bool refresh = true)
        {
            if (!ContainsFile(file))
            {
                if (predicate != null && !predicate(file))
                {
                    Log.Line(LogType.Warning, "File '{0}' is not allowed in this list", file);
                    return;
                }

                files.Add(file);
                if (refresh)
                    RefreshList();
            }
        }

        private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (FilesList.SelectedIndex > -1)
            {
                foreach (object filesListSelectedItem in FilesList.SelectedItems)
                {
                    files.Remove((string) filesListSelectedItem);
                }

                RefreshList();
            }
        }

        private void ClearButton_OnClick(object sender, RoutedEventArgs e)
        {
            files.Clear();
            RefreshList();
        }

        public bool ContainsFile(string file)
        {
            return !string.IsNullOrEmpty(file) && files.Any(x => x == file);
        }

        public void RefreshList()
        {
            if (sort)
                files = files.OrderBy(Path.GetFileNameWithoutExtension).ToList();
            FilesList.ItemsSource = null;
            FilesList.ItemsSource = files;
        }
    }
}