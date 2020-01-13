using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace GZipArchiver
{
    class StreamManager
    {
        FileStream inFileStream;
        FileStream outFileStream;

        private int blocksRead = 0;
        private int blocksWritten = 0;

        private static readonly int readBlockSize = 1024 * 1024;

        private Queue<int> writeBlockSizes = new Queue<int>();
        private Stack<int> readBlockSizes = new Stack<int>();

        public Stack<int> ReadBlockSizesStack { get { return readBlockSizes; } }

        public static int BlockSize { get { return readBlockSize; } }

        public StreamManager(FileStream inFileStream, FileStream outFileStream)
        {
            this.inFileStream = inFileStream;
            this.outFileStream = outFileStream;
        }

        public int GetNumberOfBlocksForCompression()
        {
            return (int)Math.Ceiling((double)inFileStream.Length / readBlockSize);
        }

        //Read last 4 bytre of the compressesd stream. This number of blocks
        public int GetNumberOfBlocksForDecompression()
        {
            inFileStream.Seek(-4, SeekOrigin.End);
            byte[] byteBlocksToRead = new byte[4];
            inFileStream.Read(byteBlocksToRead, 0, 4);
            int blocksToread = BitConverter.ToInt32(byteBlocksToRead, 0);
            return blocksToread;
        }

        //Read block from input stream
        public Block ReadBlock(Block block)
        {
            lock (inFileStream)
            {
                while (block.ID != blocksRead)
                {
                    Monitor.Wait(inFileStream);
                }

                int readBytes;

                try
                {
                    if ((inFileStream.Length - inFileStream.Position) < block.Size)
                    {
                        readBytes = (int)(inFileStream.Length - inFileStream.Position);
                    }
                    else
                    {
                        readBytes = block.Size;
                    }
                    block.Data = new byte[readBytes];
                    block.Size = readBytes;
                    inFileStream.Read(block.Data, 0, readBytes);
                    ++blocksRead;
                    Monitor.PulseAll(inFileStream);
                    return block;
                }
                catch(IOException ex)
                {
                    throw new Exception("Unable to read from the input file");
                }
            }
        }

        //Read block sizes from compressed stream so that we'll know what size arrays we'll need to create
        public void ReadBlockSizes()
        {
            try
            {
                int blocksToRead = GetNumberOfBlocksForDecompression();
                for (int i = 0; i < blocksToRead; i++)
                {
                    inFileStream.Seek(-4 * 2, SeekOrigin.Current);
                    byte[] byteBlockSize = new byte[4];
                    inFileStream.Read(byteBlockSize, 0, 4);
                    readBlockSizes.Push(BitConverter.ToInt32(byteBlockSize, 0));

                }
                inFileStream.Seek(0, SeekOrigin.Begin);
            }
            catch (IOException ex)
            {
                throw new Exception("Unable to read metadata from the input file\n");
            }
        }

        //Wrtie block to the output file
        public void WriteBlock(Block block)
        {
            lock (outFileStream)
            {
                while (blocksWritten != block.ID)
                {
                    Monitor.Wait(outFileStream);
                }

                try
                {
                    byte[] size = BitConverter.GetBytes(block.Data.Length);
                    outFileStream.Write(block.Data, 0, block.Data.Length);
                    ++blocksWritten;
                    writeBlockSizes.Enqueue(block.Data.Length);
                    Monitor.PulseAll(outFileStream);
                }
                catch (IOException ex)
                {
                    throw new Exception("Unable to write to the output file\n");
                }
            }
        }

        //Write block sizes and the amount of block to the end of the file for future decompression
        public void WriteBlockSizes()
        {
            try
            {
                while (writeBlockSizes.Count != 0)
                {
                    int blockSize = writeBlockSizes.Dequeue();
                    outFileStream.Write(BitConverter.GetBytes(blockSize), 0, 4);
                }
                outFileStream.Write(BitConverter.GetBytes(blocksWritten), 0, 4);
            }
            catch (IOException ex)
            {
                throw new Exception("Unable to write metadata from the output file\n");
            }
        }
    }
}
