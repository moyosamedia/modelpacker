using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using MahApps.Metro;
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

        public static string uiAccentColor => ConfigurationManager.AppSettings["accentColor"];
        public static string uiTheme => ConfigurationManager.AppSettings["theme"];

        public MainWindow()
        {
            Debug.Assert(instance == null);
            instance = this;

            InitializeComponent();

            Log.onLog = OnLogMessage;

            PopulateExportComboBoxes();
            ModelFiles.predicate = file => Utils.IsModelExtensionSupported(Path.GetExtension(file));
            TextureFiles.predicate = Utils.IsImageSupported;

            UpdateTheme();
        }

        public void UpdateTheme()
        {
            ThemeManager.ChangeAppStyle(this,
                ThemeManager.GetAccent(uiAccentColor),
                ThemeManager.GetAppTheme(uiTheme));
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
            textBlock.Inlines.Add(new LineBreak());
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

            ProcessorInfo processorInfo = CreateProcessorInfo();

            XmlSerializer serializer = new XmlSerializer(processorInfo.GetType());
            string savePath = Path.Combine(processorInfo.outputDir,
                string.Format("{0}-settings.xml", processorInfo.outputFilesPrefix));
            using (XmlWriter writer = XmlWriter.Create(savePath))
                serializer.Serialize(writer, processorInfo);

            Processor.Processor.Run(processorInfo);
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

        private ProcessorInfo CreateProcessorInfo()
        {
            string[] exportIds = ExportModelFormats.Tag as string[];
            Debug.Assert(exportIds != null);
            Debug.Assert(exportIds.Length > 0);

            return new ProcessorInfo
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
            };
        }
    }
}