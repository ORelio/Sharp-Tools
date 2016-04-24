using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace SharpTools
{
    /// <summary>
    /// Utility methods for retrieving data from SWF files
    /// </summary>
    /// <remarks>
    /// By ORelio (c) 2016 - CDDL 1.0
    /// </remarks>
    /// <seealso href="http://wwwimages.adobe.com/www.adobe.com/content/dam/Adobe/en/devnet/swf/pdf/swf-file-format-spec.pdf"/>
    public static class Swf
    {
        /// <summary>
        /// Define some common SWF tag types
        /// </summary>
        public enum TagType
        {
            FileAttributes = 69,
            BinaryData = 87
        }

        /// <summary>
        /// A SWF tag with its raw data
        /// </summary>
        public struct SwfTag
        {
            public int Type;
            public byte[] Data;
            public SwfTag(int type, byte[] data)
            {
                Type = type;
                Data = data;
            }
        }

        /// <summary>
        /// Check if the provided file is a SWF file
        /// </summary>
        /// <param name="swf">SWF file to test</param>
        /// <param name="exception">Throw an ArgumentException if the file is invalid?</param>
        /// <exception cref="ArgumentException">Thrown if requested for invalid files</exception>
        /// <returns>TRUE if the file seems to be a valid SWF file</returns>
        public static bool CheckFile(byte[] swf, bool exception = false)
        {
            if (swf == null || swf.Length < 8 || swf[1] != 'W' || swf[2] != 'S')
            {
                if (exception)
                    throw new ArgumentException("Provided file is not a valid SWF file!", "swf");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Uncompress SWF file into an uncompressed SWF file.
        /// No exception is raised if the file is already uncompressed.
        /// </summary>
        /// <param name="swf">SWF file as bytes</param>
        /// <returns>Uncompressed SWF file</returns>
        public static byte[] Uncompress(byte[] swf)
        {
            CheckFile(swf, true);
            char compression_type = (char)swf[0];
            if (compression_type == 'F')
                return swf;
            if (compression_type == 'C')
            {
                byte[] header = swf.Take(8).ToArray();
                header[0] = (byte)'F'; //Change compression type to "uncompressed"
                int size_uncompressed = header[4] + header[5] * 0x100 + header[6] * 0x10000 + header[7] * 0x1000000;
                List<byte> result = new List<byte>(header);
                result.AddRange(Zlib.Decompress(swf.Skip(8).ToArray(), size_uncompressed));
                return result.ToArray();
            }
            if (compression_type == 'Z')
                throw new NotImplementedException("This SWF file was compressed with LZMA, decompression is currently not implemented.");
            throw new ArgumentException("Got unknown compression type '" + compression_type + "'. Expecting F/C/Z.");
        }

        /// <summary>
        /// Get tags from a SWF file
        /// </summary>
        /// <remarks>
        /// An SWF file is composed of a header and a sequence of tags.
        /// The sequence of tags may be compressed, if so, it will automatically be uncompressed.
        /// </remarks>
        /// <param name="swf">SWF file as bytes</param>
        /// <returns>A list of SWF tags</returns>
        public static List<SwfTag> GetTags(byte[] swf)
        {
            swf = Uncompress(swf);

            List<SwfTag> tags = new List<SwfTag>();

            //Skip headers
            int cursorpos = 8;
            int rectFieldSizeInBits = (swf[cursorpos] >> 3) * 4 + 5;
            int rectFieldSizeInBytes = (int)Math.Ceiling(((double)rectFieldSizeInBits) / 8);
            cursorpos += (rectFieldSizeInBytes + 4); //There are 2 fields left to skip

            //Retrieve tags
            while (cursorpos < swf.Length)
            {
                try
                {
                    //TagCodeAndLength
                    ushort tagCl = BitConverter.ToUInt16(swf, cursorpos);
                    int tagLength = tagCl & 0x3F; //Lower 6 bits
                    int tagType = tagCl >> 6; //Upper 10 bits
                    cursorpos += 2;

                    //Extended length
                    if (tagLength == 0x3F)
                    {
                        tagLength = (int)BitConverter.ToUInt32(swf, cursorpos);
                        cursorpos += 4;
                    }

                    //Extract the tag
                    byte[] tag = new byte[tagLength];
                    Array.Copy(swf, cursorpos, tag, 0, tagLength);
                    tags.Add(new SwfTag(tagType, tag));
                    cursorpos += tagLength;
                }
                catch (IndexOutOfRangeException) { /* Parse error */ }
                catch (OverflowException) { /* Parse error */ }
            }
            return tags;
        }

        /// <summary>
        /// Extract raw binary data from a BinaryData SWF Tag
        /// </summary>
        /// <param name="tag">tag to extract data from</param>
        /// <returns>The binary data, with its key (characterID) and value (raw binary data)</returns>
        public static KeyValuePair<ushort, byte[]> GetBinaryData(SwfTag tag)
        {
            if (tag.Type != (int)TagType.BinaryData || tag.Data.Length < 6)
                throw new ArgumentException("The provided tag is not a binary valid data tag");
            ushort characterID = BitConverter.ToUInt16(tag.Data, 0);
            return new KeyValuePair<ushort, byte[]>(BitConverter.ToUInt16(tag.Data, 0), tag.Data.Skip(6).ToArray());
        }
    }
}
