using System;
using System.IO;
using System.Threading;

namespace GZipArchiver
{
    class Program
    {
        static int Main(string[] args)
        {
            //ApplicationSettings.Compress = true;
            //ApplicationSettings.InFile = @"D:\Downloads\123.mkv";
            //ApplicationSettings.OutFile = @"C:\Users\Aleks\Desktop\zipped.gz";

            //ApplicationSettings.Decompress = true;
            //ApplicationSettings.OutFile = @"C:\Users\Aleks\Desktop\unzipped.mkv";
            //ApplicationSettings.InFile = @"C:\Users\Aleks\Desktop\zipped.gz";

            Program pr = new Program();
            if (InputManager.ValidateInput(args))
            {

                Console.WriteLine("Program has sarted");
                int programOutput = pr.RunProgram();
                if(programOutput == 1)
                {
                    Console.WriteLine("Program encountered an error. Please, try again");
                }
                else if(programOutput == 0)
                {
                    Console.WriteLine("Program has finished succesfully!");
                }
                Console.WriteLine(programOutput);
                return (programOutput);

            }
            else
            {
                return 1;
            }
        }
        private int RunProgram()
        {
            using (var reader = new FileStream(ApplicationSettings.InFile, FileMode.Open, FileAccess.Read))
            {
                using (var writer = new FileStream(ApplicationSettings.OutFile, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    //Create input/output manager
                    StreamManager sm = new StreamManager(reader, writer);
                    int numOfTasks = 0;

                    //Get appropriate number of tasks that we'll nedd to run
                    if (ApplicationSettings.Compress)
                    {
                        numOfTasks = sm.GetNumberOfBlocksForCompression();
                    }
                    else if (ApplicationSettings.Decompress)
                    {
                        numOfTasks = sm.GetNumberOfBlocksForDecompression();
                    }

                    //Start the pool
                    ThreadPool tp = new ThreadPool(numOfTasks);
                    tp.Start();

                    //Create appropriate tasks and add them to the queue so that threads can pick tasks up
                    if (ApplicationSettings.Compress)
                    {
                        Compressor.CreateCompressionTasks(numOfTasks, tp, sm);
                    }
                    else if (ApplicationSettings.Decompress)
                    {
                        Decompressor.CreateDecompressionTasks(tp, sm);
                    }

                    //Play some animation while proccessing is happening
                    while (!tp.Finished && tp.Started)
                    {
                        Thread.Sleep(500);
                        Console.Write("\rProcessing \\ ");
                        Thread.Sleep(500);
                        Console.Write("\rProcessing | ");
                        Thread.Sleep(500);
                        Console.Write("\rProcessing / ");
                        Thread.Sleep(500);
                        Console.Write("\rProcessing | ");
                    }
                    Console.WriteLine();

                    //Add block sizes and mount of block to the end of the file
                    if (ApplicationSettings.Compress)
                    {
                        sm.WriteBlockSizes();
                    }

                    //Stop the pool
                    tp.Stop();

                    //Assert the results
                    if (tp.ExceptionHappened)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }

                }
            }
        }
    }
}
