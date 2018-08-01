namespace ModelPacker.BinPacker
{
    public class Block
    {
        public int w;
        public int h;
        public Node fit { get; internal set; }

        public Block(int w, int h)
        {
            this.w = w;
            this.h = h;
        }
    }

    public class Block<T> : Block
    {
        public T data;

        public Block(int w, int h, T data) : base(w, h)
        {
            this.data = data;
        }
    }
}