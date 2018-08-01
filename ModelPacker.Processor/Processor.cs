using System;
using System.IO;
using Assimp.Unmanaged;
using ImageMagick;
using ModelPacker.BinPacker;
using ModelPacker.Logger;

namespace ModelPacker.Processor
{
    public static class Processor
    {
        public static bool Run(ProcessorInfo info)
        {
            if (!CheckProcessorInfo(info)) return false;

            Log.Line(LogType.Debug, "Creating output directory {0}", info.outputDir);
            Directory.CreateDirectory(info.outputDir);

            if (!PackImages(info, out BinPacking imagePacker) || imagePacker.blocks == null || imagePacker.blocks.Length == 0)
            {
                return false;
            }


            return false;
        }

        private static bool PackImages(ProcessorInfo info, out BinPacking imagePacker)
        {
            Log.Line(LogType.Info, "Starting packing process for {0} images", info.textures.Length);

            bool keepTransparency = info.keepTransparency && info.textureOutputType != TextureFileType.JPG;
            MagickReadSettings readSettings = new MagickReadSettings
            {
                BackgroundColor = keepTransparency ? new MagickColor(0, 0, 0, 0) : MagickColors.Black
            };

            MagickImage[] images = new MagickImage[info.textures.Length];
            Block[] blocks = new Block[info.textures.Length];
            for (int i = 0; i < info.textures.Length; i++)
            {
                images[i] = new MagickImage(info.textures[i], readSettings);
                blocks[i] = new Block(images[i].Width, images[i].Height);
            }

            Array.Sort(images, blocks);
            Array.Reverse(images);
            Array.Reverse(blocks);

            imagePacker = new BinPacking(blocks);
            imagePacker.Fit();

            Log.Line(LogType.Debug, "BinPacker: Root size: {{ width: {0}, height: {1} }}", imagePacker.root.w,
                imagePacker.root.h);

            try
            {
                using (MagickImage finalImage = new MagickImage(
                    readSettings.BackgroundColor,
                    imagePacker.root.w,
                    imagePacker.root.h))
                {
                    for (int i = 0; i < blocks.Length; i++)
                    {
                        Block block = blocks[i];
                        if (block.fit != null)
                        {
                            Log.Line(LogType.Debug,
                                "BinPacker: {0}: {{ pos: {{ x: {1}, y: {2} }}, size: {{ width: {3}, height: {4} }}}}",
                                images[i].FileName,
                                block.fit.x, block.fit.y,
                                block.w, block.h);

                            finalImage.Composite(images[i], block.fit.x, block.fit.y, CompositeOperator.Copy);
                        }
                        else
                        {
                            Log.Line(LogType.Warning,
                                "BinPacker: Fit for '{0}' is null, it's not going to be in the final image",
                                images[i].FileName);
                        }
                    }

                    try
                    {
                        string savePath = Path.Combine(info.outputDir,
                            string.Format("{0}-packed.{1}", info.outputFilesPrefix, info.textureOutputType));
                        Log.Line(LogType.Info, "Packing images");
                        {
                            Log.Line(LogType.Info, "Saving packed image to '{0}'", savePath);
                            finalImage.Write(savePath);
                        }
                    }
                    catch (MagickOptionErrorException e)
                    {
                        Log.Exception(e);
                        return false;
                    }
                }
            }
            finally
            {
                foreach (MagickImage img in images)
                    img?.Dispose();
            }

            return true;
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

            if (string.IsNullOrEmpty(info.modelExportFormatId))
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