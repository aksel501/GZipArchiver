using System;
using System.IO;
namespace GZipArchiver
{
    static class InputManager
    {

        public static bool ValidateInput(string[] args)
        {
            if (args.Length == 3)
            {
                if(ValidateMode(args[0]) && ValidateInFile(args[1]) && ValidateOutFile(args[2]))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Console.WriteLine("Invalid number of parameters");
                return false;
            }
        }

        private static bool ValidateMode(string mode)
        {
            if (mode.ToLower() == "compress")
            {
                ApplicationSettings.Compress = true;
                ApplicationSettings.Decompress = false;
                return true;
            }
            else if (mode.ToLower() == "decompress")
            {
                ApplicationSettings.Compress = false;
                ApplicationSettings.Decompress = true;
                return true;
            }
            else
            {
                Console.WriteLine("First argument should be compress or decompress");
                return false;
            }
        }

        private static bool ValidateInFile(string inFile)
        {
            if (File.Exists(inFile))
            {
                try
                {
                    File.Open(inFile, FileMode.Open, FileAccess.Read).Dispose();
                    ApplicationSettings.InFile = inFile;
                    return true;
                }
                catch (IOException)
                {
                    Console.WriteLine("Provided input file cannot be read");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("Provided input file doesn't exist");
                return false;
            }
        }

        private static bool ValidateOutFile(string outFile)
        {
            if (File.Exists(outFile))
            {
                Console.WriteLine("Provided output file already exists");
                return false;
            }
            else if(!Directory.Exists(Path.GetDirectoryName(outFile)))
            {
                Console.WriteLine("Provided output file's directory doesn't exist");
                return false;
            }
            else if (!Path.HasExtension(outFile))
            {
                Console.WriteLine("Provided output file doesn't have an extension");
                return false;
            }
            else
            {
                ApplicationSettings.OutFile = outFile;
                return true;
            }
        }
    }
}
