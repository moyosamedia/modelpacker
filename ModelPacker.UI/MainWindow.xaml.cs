using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Assimp;
using Assimp.Unmanaged;
using ImageMagick;
using ModelPacker.Logger;

namespace ModelPacker.UI
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

            TextBlock.Inlines.Add(run);
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
            TextBlock.Text = string.Empty;

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
                if (AssimpLibrary.Instance.IsExtensionSupported(Path.GetExtension(droppedFile)))
                {
                    ModelFiles.Add(droppedFile);
                }
                else
                {
                    try
                    {
                        using (new MagickImage(droppedFile))
                        {
                        }

                        TextureFiles.Add(droppedFile);
                    }
                    catch (MagickMissingDelegateErrorException)
                    {
                        Log.Line(LogType.Warning, "Dropped file '{0}' is not supported", droppedFile);
                    }
                }
            }
        }
    }
}