using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using MahApps.Metro;
using Microsoft.Win32;
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

            ModelFiles.dialogFilter = "Models|";
            bool first = true;
            foreach (string ext in Utils.GetModelExportExtensions())
            {
                ModelFiles.dialogFilter += (!first ? ";" : "") + "*" + ext;
                first = false;
            }

            ModelFiles.dialogFilter += "|All Files|*.*";


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
#if !DEBUG
            if (Log.ShouldFilter(type, LogType.Info))
                return;
#endif
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

            if (DoCreateSettingsFile.IsChecked == true)
            {
                XmlSerializer serializer = new XmlSerializer(processorInfo.GetType());
                string savePath;
                if (!string.IsNullOrEmpty(processorInfo.outputFilesPrefix.Trim()))
                {
                    savePath = Path.Combine(processorInfo.outputDir,
                        string.Format("{0}-settings.xml", processorInfo.outputFilesPrefix));
                }
                else
                {
                    savePath = Path.Combine(processorInfo.outputDir, "settings.xml");
                }

                using (XmlWriter writer = XmlWriter.Create(savePath))
                    serializer.Serialize(writer, processorInfo);
            }

            Processor.Processor.Run(processorInfo);
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
                mergeModels = DoMergeModels.IsChecked == true,
                padding = (int) (Padding.Value ?? 0),
                keepTransparency = DoKeepTransparency.IsChecked == true,
                modelExportFormatId = exportIds[ExportModelFormats.SelectedIndex],
                textureOutputType = (TextureFileType) Enum.Parse(typeof(TextureFileType),
                    ExportTextureFormats.SelectedItem.ToString()),
                outputFilesPrefix = FilesPrefix.Text.Trim(),
                outputDir = ExportDirectory.FullPath
            };
        }

        private void PopulateFromProcessorInfo(ProcessorInfo info)
        {
            ModelFiles.files.Clear();
            foreach (string m in info.models)
                ModelFiles.Add(m, false);
            ModelFiles.RefreshList();

            TextureFiles.files.Clear();
            foreach (string t in info.textures)
                TextureFiles.Add(t, false);
            TextureFiles.RefreshList();

            DoMergeModels.IsChecked = info.mergeModels;
            Padding.Value = info.padding;
            DoKeepTransparency.IsChecked = info.keepTransparency;

            string[] exportIds = (string[]) ExportModelFormats.Tag;
            ExportModelFormats.SelectedIndex = Array.IndexOf(exportIds, info.modelExportFormatId);
            ExportTextureFormats.SelectedIndex = (int) info.textureOutputType;

            FilesPrefix.Text = info.outputFilesPrefix;
            ExportDirectory.FullPath = info.outputDir;
        }

        public void LoadSettingsFile(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ProcessorInfo));
            using (XmlReader reader = XmlReader.Create(filePath))
            {
                ProcessorInfo info = (ProcessorInfo) serializer.Deserialize(reader);
                PopulateFromProcessorInfo(info);
            }
        }

        private void OnLoadButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = System.Reflection.Assembly.GetEntryAssembly().Location,
                RestoreDirectory = true,
                Filter = "Settings File|*.xml|All Files|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                LoadSettingsFile(openFileDialog.FileName);
            }
        }

        #region DragDrop

        // From https://stackoverflow.com/questions/26195160/wpf-drag-and-drop-a-file-into-the-whole-window-titlebar-and-window-borders-inc?rq=1

        private const int WM_DROPFILES = 0x233;

        [DllImport("shell32.dll")]
        private static extern void DragAcceptFiles(IntPtr hwnd, bool fAccept);

        [DllImport("shell32.dll")]
        private static extern uint DragQueryFile(IntPtr hDrop, uint iFile, [Out] StringBuilder filename, uint cch);

        [DllImport("shell32.dll")]
        private static extern void DragFinish(IntPtr hDrop);


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            WindowInteropHelper helper = new WindowInteropHelper(this);
            IntPtr hwnd = helper.Handle;

            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);

            DragAcceptFiles(hwnd, true);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_DROPFILES)
            {
                handled = true;
                return HandleDropFiles(wParam);
            }

            return IntPtr.Zero;
        }

        private IntPtr HandleDropFiles(IntPtr hDrop)
        {
            const int MAX_PATH = 260;

            uint count = DragQueryFile(hDrop, 0xFFFFFFFF, null, 0);

            for (uint i = 0; i < count; i++)
            {
                int size = (int) DragQueryFile(hDrop, i, null, 0);

                StringBuilder filename = new StringBuilder(size + 1);
                DragQueryFile(hDrop, i, filename, MAX_PATH);

                string droppedFile = filename.ToString();

                if (Path.GetExtension(droppedFile) == ".xml")
                {
                    LoadSettingsFile(droppedFile);
                    break;
                }

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

            DragFinish(hDrop);

            ModelFiles.RefreshList();
            TextureFiles.RefreshList();

            return IntPtr.Zero;
        }

        #endregion
    }
}