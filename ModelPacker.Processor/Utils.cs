using System.Collections.Generic;
using Assimp;
using Assimp.Unmanaged;
using ImageMagick;

namespace ModelPacker.Processor
{
    public static class Utils
    {
        public static bool IsModelExtensionSupported(string ext)
        {
            return AssimpLibrary.Instance.IsExtensionSupported(ext);
        }

        public static bool IsImageSupported(string filePath)
        {
            try
            {
                using (new MagickImage(filePath))
                {
                }

                return true;
            }
            catch (MagickMissingDelegateErrorException)
            {
            }

            return false;
        }

        public static IEnumerable<ExportFormats> GetModelExportFormats()
        {
            ExportFormatDescription[] exportFormatDescriptions = AssimpLibrary.Instance.GetExportFormatDescriptions();
            foreach (ExportFormatDescription exportFormatDescription in exportFormatDescriptions)
            {
                yield return new ExportFormats
                {
                    name = string.Format("{0}, (.{1})",
                        exportFormatDescription.Description,
                        exportFormatDescription.FileExtension),
                    id = exportFormatDescription.FormatId
                };
            }
        }

        public static IEnumerable<string> GetModelExportExtensions()
        {
            ExportFormatDescription[] exportFormatDescriptions = AssimpLibrary.Instance.GetExportFormatDescriptions();
            foreach (ExportFormatDescription exportFormatDescription in exportFormatDescriptions)
            {
                yield return exportFormatDescription.FileExtension;
            }
        }

        public struct ExportFormats
        {
            public string name;
            public string id;
        }

        public static void ClampBorder(this MagickImage image, int borderSize)
        {
            IPixelCollection pixels = image.GetPixels();

            // Fill left and right borders by going row by row
            int rightOffset = image.Width - borderSize;
            byte[] targetColor;
            for (int y = borderSize; y < image.Height - borderSize; y++)
            {
                MagickGeometry left = new MagickGeometry(0, y, borderSize, 1);
                targetColor = pixels.GetValue(borderSize, y);
                pixels.SetArea(left, targetColor.RepeatArray(borderSize));

                MagickGeometry right = new MagickGeometry(rightOffset, y, borderSize, 1);
                targetColor = pixels.GetValue(rightOffset - 1, y);
                pixels.SetArea(right, targetColor.RepeatArray(borderSize));
            }

            // Fill top and bottom by going column by column
            int bottomOffset = image.Height - borderSize;
            for (int x = borderSize; x < image.Width - borderSize; x++)
            {
                MagickGeometry top = new MagickGeometry(x, 0, 1, borderSize);
                targetColor = pixels.GetValue(x, borderSize);
                pixels.SetArea(top, targetColor.RepeatArray(borderSize));

                MagickGeometry bottom = new MagickGeometry(x, bottomOffset, 1, borderSize);
                targetColor = pixels.GetValue(x, bottomOffset - 1);
                pixels.SetArea(bottom, targetColor.RepeatArray(borderSize));
            }

            int cornerSize = borderSize * borderSize;
            // Fill top left corner
            targetColor = pixels.GetValue(borderSize, borderSize);
            pixels.SetArea(0, 0, borderSize, borderSize, targetColor.RepeatArray(cornerSize));

            // Fill top right corner
            targetColor = pixels.GetValue(rightOffset - 1, borderSize);
            pixels.SetArea(rightOffset, 0, borderSize, borderSize, targetColor.RepeatArray(cornerSize));

            // Fill bottom left corner
            targetColor = pixels.GetValue(borderSize, bottomOffset - 1);
            pixels.SetArea(0, bottomOffset, borderSize, borderSize, targetColor.RepeatArray(cornerSize));

            // Fill bottom right corner
            targetColor = pixels.GetValue(rightOffset - 1, bottomOffset - 1);
            pixels.SetArea(rightOffset, bottomOffset, borderSize, borderSize, targetColor.RepeatArray(cornerSize));
        }

        public static T[] RepeatArray<T>(this T[] source, int amount)
        {
            T[] retVal = new T[source.Length * amount];
            for (int i = 0; i < retVal.Length; i += source.Length)
            {
                for (int n = 0; n < source.Length; n++)
                {
                    retVal[i + n] = source[n];
                }
            }

            return retVal;
        }
    }
}