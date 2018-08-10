using System.Collections.Generic;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using ModelPacker.Processor;

namespace ModelPacker.CMD
{
    [Verb("options")]
    public class ProcessorInfoArguments
    {
        [Option('m', "models", Separator = ';', Required = true, Min = 2, HelpText = "The models to pack.")]
        public IEnumerable<string> models { get; set; }

        [Option('t', "textures", Separator = ';', Required = true, Min = 2, HelpText
            = "The textures to pack. " +
              "It assumes there is 1 texture per 1 model file. " +
              "If there are multiple meshes in 1 file, it will assume there is 1 texture for all of them.")]
        public IEnumerable<string> textures { get; set; }

        [Option('M', "mergemodels", HelpText =
            "Should all the meshes be combined into 1 single mesh.")]
        public bool mergeModels { get; set; }

        [Option('p', "padding", HelpText = "Extra padding between the textures in pixels.")]
        public int padding { get; set; }

        [Option('a', "keeptransparency", HelpText =
            "Should we keep the transparency in the output texture. If using JPG as 'textureOutputType' this will be ignored.")]
        public bool keepTransparency { get; set; }

        [Option('i', "textureexportformat", HelpText =
            "What the format of the output texture should be (PNG/JPG/HDR/TIFF).")]
        public TextureFileType textureOutputType { get; set; }

        [Option('e', "modelexportformat", Required = true,
            HelpText = "The format in which the model should be exported e.g. obj or fbx.")]
        public string modelExportFormatId { get; set; }

        [Option('p', "outputfilesprefix", Required = true, HelpText = "The prefix that all the files should get.")]
        public string outputFilesPrefix { get; set; }

        [Option('o', "outputdir", Required = true, HelpText = "Where all the output files should go.")]
        public string outputDir { get; set; }

        [Option('n', "nosettingsfile", HelpText =
            "Enabling this will prevent it from creating the settings file in the output directory.")]
        public bool noSettingsFile { get; set; }

        [Usage(ApplicationAlias = "ModelPacker.CMD.exe")]
        public static IEnumerable<Example> examples
        {
            get
            {
                yield return new Example("Default", new ProcessorInfo
                {
                    models = new[] {"model1.fbx", "model2.obj"},
                    textures = new[] {"texture1.jpg", "texture2.png"},
                    padding = 10,
                    mergeModels = true,
                    keepTransparency = true,
                    textureOutputType = TextureFileType.JPG,
                    modelExportFormatId = "obj",
                    outputFilesPrefix = "newpack",
                    outputDir = "Packed output/"
                });
            }
        }

        public static implicit operator ProcessorInfo(ProcessorInfoArguments options)
        {
            return new ProcessorInfo
            {
                models = options.models.ToArray(),
                textures = options.textures.ToArray(),
                mergeModels = options.mergeModels,
                padding = options.padding,
                keepTransparency = options.keepTransparency,
                textureOutputType = options.textureOutputType,
                modelExportFormatId = options.modelExportFormatId,
                outputFilesPrefix = options.outputFilesPrefix,
                outputDir = options.outputDir
            };
        }
    }

    [Verb("from-file", HelpText = "Load settings from a settings file")]
    public class SettingsFileArgument
    {
        [Option('f', "file", Required = true, HelpText = "Path to the settings file")]
        public string file { get; set; }

        [Usage(ApplicationAlias = "ModelPacker.CMD.exe")]
        public static IEnumerable<Example> examples
        {
            get
            {
                yield return new Example("Default", new SettingsFileArgument
                {
                    file = "myTest-settings.xml"
                });
            }
        }
    }
}