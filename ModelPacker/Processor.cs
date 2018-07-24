using System.IO;
using Assimp.Unmanaged;
using ImageMagick;
using ModelPacker.Logger;

namespace ModelPacker
{
    public static class Processor
    {
        public static bool Run(ProcessorInfo info)
        {
            if (!CheckProcessorInfo(info)) return false;

            Log.Line(LogType.Debug, "Creating output directory {0}", info.outputDir);
            Directory.CreateDirectory(info.outputDir);

            PackImages(info);

            return false;
        }

        private static void PackImages(ProcessorInfo info)
        {
            Log.Line(LogType.Debug, "Packing {0} images", info.textures.Length);

            MagickNET.SetLogEvents(LogEvents.All);
            MagickNET.Log += MagickNetOnLog;

            using (MagickImageCollection images = new MagickImageCollection())
            {
                MontageSettings montageSettings = new MontageSettings
                {
                    BackgroundColor = MagickColor.FromRgb(0, 0, 0),
                    BorderWidth = 0
                };

                foreach (string file in info.textures)
                {
                    images.Add(file);
                }

                using (IMagickImage result = images.Montage(montageSettings))
                {
                    string savePath = Path.Combine(info.outputDir,
                        string.Format("{0}-packed.png", info.outputFilesPrefix));
                    Log.Line(LogType.Info, "Saving packed image to '{0}'", savePath);
                    result.Write(savePath);
                }
            }
        }

        private static void MagickNetOnLog(object sender, LogEventArgs e)
        {
            Log.Line(LogType.Debug, string.Format("Magick.NET: {0}: {1}", e.EventType, e.Message));
        }

        private static bool CheckProcessorInfo(ProcessorInfo info)
        {
            if (info.models == null || info.models.Length == 0)
            {
                Log.Line(LogType.Error, "There were no models supplied");
                return false;
            }

            if (info.textures == null || info.textures.Length == 0)
            {
                Log.Line(LogType.Error, "There were no textures supplied");
                return false;
            }

            if (info.textures.Length != info.models.Length)
            {
                Log.Line(LogType.Error, "The amount of textures does not match the amount of models");
                return false;
            }

            if (string.IsNullOrEmpty(info.exportFormatId))
            {
                Log.Line(LogType.Error, "Export format is null");
                return false;
            }

            if (string.IsNullOrEmpty(info.outputFilesPrefix))
            {
                Log.Line(LogType.Error, "Output files prefix was not supplied");
                return false;
            }

            if (string.IsNullOrEmpty(info.outputDir))
            {
                Log.Line(LogType.Error, "Output directory was not supplied");
                return false;
            }

            foreach (string file in info.models)
            {
                if (!File.Exists(file))
                {
                    Log.Line(LogType.Error, "Model file at '{0}' doesn't exist", file);
                    return false;
                }

                if (!AssimpLibrary.Instance.IsExtensionSupported(Path.GetExtension(file)))
                {
                    Log.Line(LogType.Error, "Model of type '{0}' is not supported", Path.GetExtension(file));
                    return false;
                }
            }

            foreach (string file in info.textures)
            {
                if (!File.Exists(file))
                {
                    Log.Line(LogType.Error, "Texture file at '{0}' doesn't exist", file);
                    return false;
                }

                try
                {
                    using (new MagickImage(file))
                    {
                    }
                }
                catch (MagickMissingDelegateErrorException)
                {
                    Log.Line(LogType.Error, "Texture file format at '{0}' is not supported", file);
                    return false;
                }
            }


            return true;
        }
    }
}