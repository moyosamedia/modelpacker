namespace ModelPacker.BinPacker
{
    public class BinPacking
    {
        public Node root { get; private set; }
        
        public Block[] blocks { get; }

        public BinPacking(Block[] blocks)
        {
            this.blocks = blocks;
        }

        public void Fit()
        {
            root = new Node
            {
                x = 0,
                y = 0,
                w = blocks.Length > 0 ? blocks[0].w : 0,
                h = blocks.Length > 0 ? blocks[0].h : 0
            };

            Node node;
            foreach (Block block in blocks)
            {
                node = FindNode(root, block.w, block.h);
                if (node != null)
                    block.fit = SplitNode(node, block.w, block.h);
                else
                    block.fit = GrowNode(block.w, block.h);
            }
        }

        private static Node FindNode(Node root, int w, int h)
        {
            if (root.used)
                return FindNode(root.right, w, h) ?? FindNode(root.down, w, h);
            if (w <= root.w && h <= root.h)
                return root;

            return null;
        }

        private static Node SplitNode(Node node, int w, int h)
        {
            node.used = true;
            node.down = new Node
            {
                x = node.x,
                y = node.y + h,
                w = node.w,
                h = node.h - h
            };
            node.right = new Node
            {
                x = node.x + w,
                y = node.y,
                w = node.w - w,
                h = h
            };
            return node;
        }

        private Node GrowNode(int w, int h)
        {
            bool canGrowDown = w <= root.w;
            bool canGrowRight = h <= root.h;

            bool shouldGrowRight = canGrowRight && root.h >= root.w + w;
            bool shouldGrowDown = canGrowDown && root.w >= root.h + h;

            if (shouldGrowRight)
                return GrowRight(w, h);
            if (shouldGrowDown)
                return GrowDown(w, h);
            if (canGrowRight)
                return GrowRight(w, h);
            if (canGrowDown)
                return GrowDown(w, h);

            return null;
        }

        private Node GrowRight(int w, int h)
        {
            root = new Node
            {
                used = true,
                w = root.w + w,
                h = root.h,
                down = root,
                right = new Node
                {
                    x = root.w,
                    y = 0,
                    w = w,
                    h = root.h
                }
            };

            Node node = FindNode(root, w, h);
            return SplitNode(node, w, h);
        }

        public Node GrowDown(int w, int h)
        {
            root = new Node
            {
                used = true,
                w = root.w,
                h = root.h + h,
                down = new Node
                {
                    x = 0,
                    y = root.h,
                    w = root.w,
                    h = h
                },
                right = root
            };
            Node node = FindNode(root, w, h);
            return SplitNode(node, w, h);
        }
    }
}