namespace ModelPacker
{
    public struct ProcessorInfo
    {
        public string[] models;
        public string[] textures;
        public bool shouldMergeModels;
        public string exportFormatId;
        public string outputFilesPrefix;
        public string outputDir;
    }
}