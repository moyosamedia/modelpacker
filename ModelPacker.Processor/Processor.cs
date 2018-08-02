using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assimp;
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

            if (!PackImages(info, out BinPacking imagePacker) || imagePacker.blocks == null ||
                imagePacker.blocks.Length == 0)
            {
                Log.Line(LogType.Error, "Failed to process the textures");
                return false;
            }

            if (!PackModels(info, imagePacker))
            {
                Log.Line(LogType.Error, "Failed to process the models");
                return false;
            }

            Log.Line(LogType.Info, "Finished processing files");

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
                            string.Format("{0}-textures.{1}", info.outputFilesPrefix, info.textureOutputType));
                        Log.Line(LogType.Info, "Saving packed image to '{0}'", savePath);
                        finalImage.Write(savePath);
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

        private static bool PackModels(ProcessorInfo info, BinPacking imagePacker)
        {
            using (AssimpContext importer = new AssimpContext())
            {
                ExportFormatDescription[] exportFormatDescriptions = importer.GetSupportedExportFormats();

                Log.Line(LogType.Info, "Shifting uv(w) positions of {0} models to their new location",
                    info.models.Length);

                for (int i = 0; i < info.models.Length; i++)
                {
                    string modelPath = info.models[i];
                    Scene model = importer.ImportFile(modelPath);
                    if (!model.HasMeshes)
                    {
                        Log.Line(LogType.Warning, "There are no meshes in model file '{0}'", modelPath);
                        continue;
                    }

                    Log.Line(LogType.Info, "Processing {0} meshes from model file '{1}'", model.MeshCount, modelPath);
                    foreach (Mesh mesh in model.Meshes)
                    {
                        Log.Line(LogType.Debug, "Found {0} uv(w) channels in mesh '{1}'",
                            mesh.TextureCoordinateChannelCount, mesh.Name);

                        // TODO: Add support for offsetting uvs per mesh instead of per model file
                        // Thus having 1 texture per mesh instead of having 1 texture per model file
                        float scaleX = imagePacker.blocks[i].w / (float) imagePacker.root.w;
                        float scaleY = imagePacker.blocks[i].h / (float) imagePacker.root.h;
                        float offsetX = imagePacker.blocks[i].fit.x / (float) imagePacker.root.w;
                        float offsetY = imagePacker.blocks[i].fit.y / (float) imagePacker.root.h;

                        Log.Line(LogType.Debug, "Calculated scaling multipliers: x: {0}; y: {1}", scaleX, scaleY);
                        Log.Line(LogType.Debug, "Calculated offsets: x: {0}; y: {1}", offsetX, offsetY);

                        for (int uvwChannel = 0; uvwChannel < mesh.TextureCoordinateChannelCount; uvwChannel++)
                        {
                            List<Vector3D> uvw = mesh.TextureCoordinateChannels[uvwChannel];
                            if (uvw == null || uvw.Count == 0)
                                continue;

                            for (int n = 0; n < uvw.Count; n++)
                            {
                                uvw[n] = new Vector3D(
                                    uvw[n].X * scaleX + offsetX,
                                    uvw[n].Y * scaleY + offsetY,
                                    uvw[n].Z);
                            }

                            mesh.TextureCoordinateChannels[uvwChannel] = uvw;
                        }
                    }
                    
                    // TODO: Add merge models functionality

                    ExportFormatDescription exportformat = exportFormatDescriptions.FirstOrDefault(x =>
                        x.FormatId == info.modelExportFormatId);

                    if (exportformat == null)
                    {
                        Log.Line(LogType.Error, "Model export format {0} is not supported!", info.modelExportFormatId);
                        return false;
                    }

                    string savePath = Path.Combine(info.outputDir,
                        string.Format("{0}-{1}.{2}",
                            info.outputFilesPrefix,
                            Path.GetFileNameWithoutExtension(modelPath),
                            exportformat.FileExtension));

                    Log.Line(LogType.Info, "Saving edited model to '{0}'", savePath);
                    importer.ExportFile(model, savePath, info.modelExportFormatId);
                }
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