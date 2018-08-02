namespace ModelPacker.Processor
{
    public struct ProcessorInfo
    {
        public string[] models;
        public string[] textures;
        internal bool mergeModels;
        public bool keepTransparency;
        public TextureFileType textureOutputType;
        public string modelExportFormatId;
        public string outputFilesPrefix;
        public string outputDir;
    }
}