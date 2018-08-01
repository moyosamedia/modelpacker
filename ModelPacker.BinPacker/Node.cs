namespace ModelPacker.BinPacker
{
    public class Node
    {
        public int x { get; internal set; }
        public int y { get; internal set; }
        public int w { get; internal set; }
        public int h { get; internal set; }
        public bool used { get; internal set; }
        public Node down { get; internal set; }
        public Node right { get; internal set; }
    }
}