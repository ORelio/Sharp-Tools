using System.IO;
using System.IO.Compression;

namespace SharpTools
{
    /// <summary>
    /// Simplified GZip file and stream handling.
    /// </summary>
    public static class GZip
    {
        /// <summary>
        /// Compress data from a file using GZip algorithm to another file
        /// </summary>
        /// <param name="inputFile">File to compress</param>
        /// <param name="outputFile">Compressed file</param>
        public static void Compress(string inputFile, string outputFile)
        {
            Stream fIn = File.OpenRead(inputFile);
            Stream fOut = new GZipStream(File.OpenWrite(outputFile), CompressionMode.Compress);
            fIn.CopyTo(fOut);
            fIn.Close();
            fOut.Close();
        }

        /// <summary>
        /// Wrap the given stream into a GZip auto-compress stream
        /// </summary>
        /// <param name="outputStream"></param>
        public static Stream CompressTo(Stream outputStream)
        {
            return new GZipStream(outputStream, CompressionMode.Compress);
        }

        /// <summary>
        /// Decompress GZipped data from a stream
        /// </summary>
        /// <param name="inputStream">Stream to decompress</param>
        /// <returns>A stream in which decompressed data can be read</returns>
        public static Stream Decompress(Stream inputStream)
        {
            return new GZipStream(inputStream, CompressionMode.Decompress);
        }

        /// <summary>
        /// Decompress GZipped data from a file
        /// </summary>
        /// <param name="inputFile">File to decompress</param>
        /// <returns>A stream in which decompressed data can be read</returns>
        public static Stream Decompress(string inputFile)
        {
            return Decompress(File.OpenRead(inputFile));
        }

        /// <summary>
        /// Decompress GZipped data from a file to another file
        /// </summary>
        /// <param name="inputFile">File to decompress</param>
        /// /// <param name="outputFile">Decompressed file</param>
        public static void Decompress(string inputFile, string outputFile)
        {
            Stream fOut = File.OpenWrite(outputFile);
            Stream fIn = Decompress(inputFile);
            fIn.CopyTo(fOut);
            fOut.Close();
            fIn.Close();
        }
    }
}
