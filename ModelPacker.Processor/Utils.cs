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

        public struct ExportFormats
        {
            public string name;
            public string id;
        }
    }
}