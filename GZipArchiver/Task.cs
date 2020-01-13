using System;

namespace GZipArchiver
{
    class Task
    {
        private Block block;
        private StreamManager streamManager;

        public Task(Block block, StreamManager streamManager)
        {
            this.block = block;
            this.streamManager = streamManager;
        }

        public void StartTask()
        {
            try
            {
                block = streamManager.ReadBlock(block);
                if (ApplicationSettings.Compress)
                {
                    block = Compressor.Compress(block);
                }
                else if (ApplicationSettings.Decompress)
                {
                    block = Decompressor.Decompress(block);
                }
                streamManager.WriteBlock(block);
                block = null;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
