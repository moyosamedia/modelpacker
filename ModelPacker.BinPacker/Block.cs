namespace ModelPacker.BinPacker
{
    public class Block<T>
    {
        public int w;
        public int h;
        public Node fit { get; internal set; }
        public T data;

        public Block(int w, int h, T data)
        {
            this.w = w;
            this.h = h;
            this.data = data;
        }

        public Block(int w, int h)
        {
            this.w = w;
            this.h = h;
        }
    }
}