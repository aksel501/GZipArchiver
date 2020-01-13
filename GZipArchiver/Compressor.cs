using System;
using System.IO;
using System.IO.Compression;

namespace GZipArchiver
{
    class Compressor
    {
        public static void CreateCompressionTasks(int numOfTasks, ThreadPool tp, StreamManager sm)
        {
            for (int i = 0; i < numOfTasks; i++)
            {
                Block block = new Block(StreamManager.BlockSize);
                Task task = new Task(block, sm);
                tp.AddTaskToQueue(task);
            }
        }

        public static Block Compress(Block block)
        {
            using (MemoryStream memStream = new MemoryStream(block.Data.Length))
            {
                using (GZipStream compressionStream = new GZipStream(memStream, CompressionMode.Compress))
                {
                    compressionStream.Write(block.Data, 0, block.Data.Length);
                }
                block.Data = memStream.ToArray();
            }
            return block;
        }
    }
}
