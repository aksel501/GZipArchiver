using System;
using System.IO;
using System.IO.Compression;

namespace GZipArchiver
{
    class Decompressor
    {
        public static void CreateDecompressionTasks(ThreadPool tp, StreamManager sm)
        {
            sm.ReadBlockSizes();
            while (sm.ReadBlockSizesStack.Count != 0)
            {
                int blockSize = sm.ReadBlockSizesStack.Pop();
                Block block = new Block(blockSize);
                Task task = new Task(block, sm);
                tp.AddTaskToQueue(task);
            }
        }

        public static Block Decompress(Block block)
        {
            using (MemoryStream memStream = new MemoryStream(StreamManager.BlockSize))
            {
                using (MemoryStream blockToDecompress = new MemoryStream(block.Data, 0, block.Data.Length))
                {
                    using (GZipStream decompressionStream = new GZipStream(blockToDecompress, CompressionMode.Decompress))
                    {
                        byte[] bytes = new byte[StreamManager.BlockSize];
                        int bytesRead;
                        bytesRead = decompressionStream.Read(bytes, 0, bytes.Length);
                        memStream.Write(bytes, 0, bytesRead);

                    }
                    block.Data = memStream.ToArray();
                }
            }
            return block;
        }
    }
}
