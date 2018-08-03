using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ModelPacker.Logger;
using ModelPacker.Processor;

namespace ModelPacker.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static MainWindow instance;

        public static TextBlock textBlock => instance.InfoText;

        public MainWindow()
        {
            Debug.Assert(instance == null);
            instance = this;

            InitializeComponent();

            PopulateExportComboBoxes();

            Log.onLog = OnLogMessage;
        }

        private void OnLogMessage(LogType type, string message)
        {
            Run run = new Run(string.Format("[{0}] {1}", type, message));
            switch (type)
            {
                case LogType.Debug:
                case LogType.Info:
                    break;
                case LogType.Warning:
                    run.Foreground = Brushes.Orange;
                    break;
                case LogType.Error:
                    run.Foreground = Brushes.DarkRed;
                    run.FontWeight = FontWeights.Bold;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            textBlock.Inlines.Add(run);
        }

        private void PopulateExportComboBoxes()
        {
            // Models
            ExportModelFormats.Items.Clear();

            IEnumerable<Utils.ExportFormats> modelExportFormats = Utils.GetModelExportFormats();
            ExportModelFormats.Tag = modelExportFormats.Select(x => x.id).ToArray();
            Debug.Assert(ExportModelFormats.Tag != null);

            foreach (Utils.ExportFormats format in modelExportFormats)
            {
                ExportModelFormats.Items.Add(format.name);
            }

            ExportModelFormats.SelectedIndex = 0;

            // Textures
            ExportTextureFormats.Items.Clear();
            foreach (string name in Enum.GetNames(typeof(TextureFileType)))
                ExportTextureFormats.Items.Add(name);
            ExportTextureFormats.SelectedIndex = 0;
        }

        private void OnExportButtonClick(object sender, RoutedEventArgs e)
        {
            textBlock.Text = string.Empty;

            string[] exportIds = ExportModelFormats.Tag as string[];
            Debug.Assert(exportIds != null);
            Debug.Assert(exportIds.Length > 0);

            Processor.Processor.Run(new ProcessorInfo
            {
                models = ModelFiles.files.ToArray(),
                textures = TextureFiles.files.ToArray(),
                //mergeModels = DoMergeFiles.IsChecked == true,
                keepTransparency = DoKeepTransparency.IsChecked == true,
                modelExportFormatId = exportIds[ExportModelFormats.SelectedIndex],
                textureOutputType = (TextureFileType) Enum.Parse(typeof(TextureFileType),
                    ExportTextureFormats.SelectedItem.ToString()),
                outputFilesPrefix = FilesPrefix.Text,
                outputDir = ExportDirectory.FullPath
            });
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[] droppedFiles = null;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                droppedFiles = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            }

            if (null == droppedFiles || !droppedFiles.Any())
            {
                return;
            }

            foreach (string droppedFile in droppedFiles)
            {
                if (Utils.IsModelExtensionSupported(Path.GetExtension(droppedFile)))
                {
                    ModelFiles.Add(droppedFile, false);
                }
                else
                {
                    if (Utils.IsImageSupported(droppedFile))
                    {
                        TextureFiles.Add(droppedFile, false);
                    }
                    else
                    {
                        Log.Line(LogType.Warning, "Dropped file '{0}' is not supported", droppedFile);
                    }
                }
            }
            ModelFiles.RefreshList();
            TextureFiles.RefreshList();
        }
    }
}