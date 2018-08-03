using System.Collections.Generic;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using ModelPacker.Processor;

namespace ModelPacker.CMD
{
    public class ArgumentOptions
    {
        [Option(Separator = ';', Min = 2, HelpText = "The models to pack.")]
        public IEnumerable<string> models { get; set; }

        [Option(Separator = ';', Min = 2, HelpText
            = "The textures to pack. " +
              "It assumes there is 1 texture per 1 model file. " +
              "If there are multiple meshes in 1 file, it will assume there 1 texture for all of them.")]

        public IEnumerable<string> textures { get; set; }

        [Option(HelpText =
            "Should we keep the transparency in the output texture. If using JPG as 'textureOutputType' this will be ignored.")]
        public bool keepTransparency { get; set; }

        [Option(Required = true, HelpText = "What the format of the output texture should be (PNG/JPG/HDR/TIFF).")]
        public TextureFileType textureOutputType { get; set; }

        [Option(Required = true, HelpText = "The format in which the model should be exported e.g. obj or fbx.")]
        public string modelExportFormatId { get; set; }

        [Option(Required = true, HelpText = "The prefix that all the files should get.")]
        public string outputFilesPrefix { get; set; }

        [Option(Required = true, HelpText = "Where all the output files should go.")]
        public string outputDir { get; set; }

        [Usage(ApplicationAlias = "ModelPacker")]
        public static IEnumerable<Example> examples
        {
            get
            {
                yield return new Example("Default", new ProcessorInfo
                {
                    models = new[] {"model1.fbx", "model2.obj"},
                    textures = new[] {"texture1.jpg", "texture2.png"},
                    keepTransparency = true,
                    textureOutputType = TextureFileType.JPG,
                    modelExportFormatId = "obj",
                    outputFilesPrefix = "newpack",
                    outputDir = "Packed output/"
                });
            }
        }

        public static implicit operator ProcessorInfo(ArgumentOptions options)
        {
            return new ProcessorInfo
            {
                models = options.models.ToArray(),
                textures = options.textures.ToArray(),
                keepTransparency = options.keepTransparency,
                textureOutputType = options.textureOutputType,
                modelExportFormatId = options.modelExportFormatId,
                outputFilesPrefix = options.outputFilesPrefix,
                outputDir = options.outputDir
            };
        }
    }
}