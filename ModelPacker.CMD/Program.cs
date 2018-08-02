using System;
using ModelPacker.Logger;
using ModelPacker.Processor;

namespace ModelPacker.CMD
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Log.onLog = OnLog;

            ProcessorInfo processorInfo = new ProcessorInfo
            {
                models = new[]
                {
                    @"D:\TEMP\GearVrController.fbx",
                    @"D:\TEMP\tree_1_3.FBX",
                    @"D:\TEMP\tree_1_3.FBX",
                    @"D:\TEMP\tree_1_3.FBX"
                },
                textures = new[]
                {
                    @"D:\TEMP\GearVrController_color_128.tif",
                    @"D:\TEMP\T0U6VPAN8-U45969Z62-bddd70681243-512.png",
                    @"D:\TEMP\tree_leaf_transparent_2.png",
                    @"D:\TEMP\GearVrController_color_128.tif"
                },
                //mergeModels = true,
                keepTransparency = true,
                textureOutputType = TextureFileType.PNG,
                modelExportFormatId = "obj",
                outputFilesPrefix = "test",
                outputDir = @"D:\TEMP\"
            };
            try
            {
                Processor.Processor.Run(processorInfo);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            
            Log.Line(LogType.Info, "Done");
        }

        private static void OnLog(LogType type, string message)
        {
            if (Log.ShouldFilter(type, LogType.Debug))
                return;
            
            switch (type)
            {
                case LogType.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
            }

            Console.WriteLine("[{0}]: {1}", type, message);
            Console.ResetColor();
        }
    }
}