

namespace GZipArchiver
{
    static class ApplicationSettings
    {

        static bool compress;
        static bool decompress;

        static string inFile;
        static string outFile;

        public static bool Compress { get { return compress; } set { compress = value; } }
        public static bool Decompress { get { return decompress; } set { decompress = value; } }
        public static string InFile { get { return inFile; } set { inFile = value; } }
        public static string OutFile { get { return outFile; } set { outFile = value; } }
    }
}
