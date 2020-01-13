
namespace GZipArchiver
{
    class Block
    {
        private int id;
        private byte[] data;
        private int size;

        private static int blockCounter = 0;

        public int ID { get { return id; } }

        //Has to be stored separately as creating an array of the size wastes too much memory
        public int Size { get { return size; } set { size = value; } }
        public byte[] Data { get { return data; } set { data = value; } }

        public Block(int size)
        {
            this.size = size;
            this.id = blockCounter++;
        }
    }
}
