namespace ModelPacker.Processor
{
    public struct ProcessorInfo
    {
        public string[] models;
        public string[] textures;
        public bool shouldMergeModels;
        public bool keepTransparency;
        public string modelExportFormatId;
        public string outputFilesPrefix;
        public string outputDir;
    }
}