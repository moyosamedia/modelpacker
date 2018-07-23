using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Assimp;
using Assimp.Unmanaged;
using ModelPacker.CustomControls;

namespace ModelPacker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static MainWindow instance;

        public static TextBlock TextBlock => instance.InfoText;

        public MainWindow()
        {
            Debug.Assert(instance == null);
            instance = this;

            InitializeComponent();

            PopulateExportComboBox();
        }

        private void PopulateExportComboBox()
        {
            ExportFormatDescription[] exportFormatDescriptions = AssimpLibrary.Instance.GetExportFormatDescriptions();
            ExportFormats.Items.Clear();
            ExportFormats.Tag = exportFormatDescriptions.Select(x => x.FormatId).ToArray();
            Debug.Assert(ExportFormats.Tag != null);

            foreach (ExportFormatDescription exportFormatDescription in exportFormatDescriptions)
            {
                ExportFormats.Items.Add(string.Format("{0}, (.{1})",
                    exportFormatDescription.Description,
                    exportFormatDescription.FileExtension));
            }

            ExportFormats.SelectedIndex = 0;
        }

        private void OnExportButtonClick(object sender, RoutedEventArgs e)
        {
            Log.Clear();
            string[] exportIds = ExportFormats.Tag as string[];
            Debug.Assert(exportIds != null);
            Debug.Assert(exportIds.Length > 0);

            Processor.Run(new ProcessorInfo
            {
                models = ModelFiles.files,
                textures = TextureFiles.files,
                shouldMergeModels = DoMergeFiles.IsChecked == true,
                exportFormatId = exportIds[ExportFormats.SelectedIndex],
                outputFilesPrefix = FilesPrefix.Text,
                outputDir = ExportDirectory.FullPath
            });
        }
    }
}