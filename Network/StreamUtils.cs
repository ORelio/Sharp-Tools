using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace SharpTools
{
    /// <summary>
    /// Stream reading utilities - By ORelio (c) 2016-2018 - CDDL 1.0
    /// </summary>
    public static class StreamUtils
    {
        // ================ //
        //  READ UTILITIES  //
        // ================ //

        /// <summary>
        /// Read a byte array from the specified stream
        /// </summary>
        public static byte[] readBytes(Stream stream, int length)
        {
            byte[] buffer = new byte[length];
            int read = 0;
            while (read < length)
            {
                int readTmp = stream.Read(buffer, read, length - read);
                if (readTmp == 0)
                    throw new EndOfStreamException(String.Format("Could read {0} byte(s) from the stream when requested to read {1} byte(s).", read, length));
                read += readTmp;
            }
            return buffer;
        }

        /// <summary>
        /// Read a string from the specified stream. Default encoding is UTF-8. Expects length in bytes, not characters.
        /// </summary>
        public static string readString(Stream stream, int length, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            if (length > 0)
            {
                return Encoding.UTF8.GetString(readBytes(stream, length));
            }
            else return "";
        }

        /// <summary>
        /// Read a single-byte boolean from the specified stream.
        /// </summary>
        public static bool readBool(Stream stream)
        {
            return stream.ReadByte() != 0x00;
        }

        /// <summary>
        /// Read a 2-bytes integer from the specified stream.
        /// </summary>
        public static short readShort(Stream stream, bool bigEndian = true)
        {
            byte[] rawValue = readBytes(stream, 2);
            if (bigEndian)
                Array.Reverse(rawValue);
            return BitConverter.ToInt16(rawValue, 0);
        }

        /// <summary>
        /// Read a 4-bytes integer from the specified stream.
        /// </summary>
        public static int readInt(Stream stream, bool bigEndian = true)
        {
            byte[] rawValue = readBytes(stream, 4);
            if (bigEndian)
                Array.Reverse(rawValue);
            return BitConverter.ToInt32(rawValue, 0);
        }

        /// <summary>
        /// Read a 8-bytes integer from the specified stream.
        /// </summary>
        public static long readLong(Stream stream, bool bigEndian = true)
        {
            byte[] rawValue = readBytes(stream, 8);
            if (bigEndian)
                Array.Reverse(rawValue);
            return BitConverter.ToInt64(rawValue, 0);
        }

        /// <summary>
        /// Read an unsigned 2-bytes integer from the specified stream.
        /// </summary>
        public static ushort readUShort(Stream stream, bool bigEndian = true)
        {
            byte[] rawValue = readBytes(stream, 2);
            if (bigEndian)
                Array.Reverse(rawValue);
            return BitConverter.ToUInt16(rawValue, 0);
        }

        /// <summary>
        /// Read an unsigned 4-bytes integer from the specified stream.
        /// </summary>
        public static uint readUInt(Stream stream, bool bigEndian = true)
        {
            byte[] rawValue = readBytes(stream, 4);
            if (bigEndian)
                Array.Reverse(rawValue);
            return BitConverter.ToUInt32(rawValue, 0);
        }

        /// <summary>
        /// Read an unsigned 8-bytes integer from the specified stream.
        /// </summary>
        public static ulong readULong(Stream stream, bool bigEndian = true)
        {
            byte[] rawValue = readBytes(stream, 8);
            if (bigEndian)
                Array.Reverse(rawValue);
            return BitConverter.ToUInt64(rawValue, 0);
        }

        /// <summary>
        /// Read a Guid/UUID from from the specified stream.
        /// </summary>
        public static Guid readGuid(Stream stream, bool bigEndian = true)
        {
            byte[] rawValue = readBytes(stream, 16);
            if (bigEndian)
                Array.Reverse(rawValue);
            return new Guid(rawValue);
        }

        /// <summary>
        /// Read a variable-length integer from the specified stream.
        /// </summary>
        public static int readVarInt(Stream stream)
        {
            int i = 0;
            int j = 0;
            int k = 0;
            while (true)
            {
                k = (byte)stream.ReadByte();
                i |= (k & 0x7F) << j++ * 7;
                if (j > 5) throw new OverflowException("VarInt too big");
                if ((k & 0x80) != 128) break;
            }
            return i;
        }

        // ================= //
        //  WRITE UTILITIES  //
        // ================= //

        /// <summary>
        /// Write a byte array to the specified stream
        /// </summary>
        public static void writeBytes(Stream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Write a string to the specified stream. Default encoding is UTF-8.
        /// </summary>
        public static void writeString(Stream stream, string value, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            writeBytes(stream, Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// Write a single-byte boolean to the specified stream.
        /// </summary>
        public static void writeBool(Stream stream, bool value)
        {
            if (value)
                stream.WriteByte(0x01);
            else stream.WriteByte(0x00);
        }

        /// <summary>
        /// Write a 2-bytes integer to the specified stream.
        /// </summary>
        public static void writeShort(Stream stream, short value, bool bigEndian = true)
        {
            byte[] rawValue = BitConverter.GetBytes(value);
            if (bigEndian)
                Array.Reverse(rawValue);
            writeBytes(stream, rawValue);
        }

        /// <summary>
        /// Write a 4-bytes integer to the specified stream.
        /// </summary>
        public static void writeInt(Stream stream, int value, bool bigEndian = true)
        {
            byte[] rawValue = BitConverter.GetBytes(value);
            if (bigEndian)
                Array.Reverse(rawValue);
            writeBytes(stream, rawValue);
        }

        /// <summary>
        /// Write a 8-bytes integer to the specified stream.
        /// </summary>
        public static void writeLong(Stream stream, long value, bool bigEndian = true)
        {
            byte[] rawValue = BitConverter.GetBytes(value);
            if (bigEndian)
                Array.Reverse(rawValue);
            writeBytes(stream, rawValue);
        }

        /// <summary>
        /// Write an unsigned 2-bytes integer to the specified stream.
        /// </summary>
        public static void writeUShort(Stream stream, ushort value, bool bigEndian = true)
        {
            byte[] rawValue = BitConverter.GetBytes(value);
            if (bigEndian)
                Array.Reverse(rawValue);
            writeBytes(stream, rawValue);
        }

        /// <summary>
        /// Write an unsigned 4-bytes integer to the specified stream.
        /// </summary>
        public static void writeUInt(Stream stream, uint value, bool bigEndian = true)
        {
            byte[] rawValue = BitConverter.GetBytes(value);
            if (bigEndian)
                Array.Reverse(rawValue);
            writeBytes(stream, rawValue);
        }

        /// <summary>
        /// Write an unsigned 8-bytes integer to the specified stream.
        /// </summary>
        public static void writeULong(Stream stream, ulong value, bool bigEndian = true)
        {
            byte[] rawValue = BitConverter.GetBytes(value);
            if (bigEndian)
                Array.Reverse(rawValue);
            writeBytes(stream, rawValue);
        }

        /// <summary>
        /// Write a Guid/UUID from to the specified stream.
        /// </summary>
        public static void writeGuid(Stream stream, Guid guid, bool bigEndian = true)
        {
            byte[] rawValue = guid.ToByteArray();
            if (bigEndian)
                Array.Reverse(rawValue);
            writeBytes(stream, rawValue);
        }

        /// <summary>
        /// Write a variable-length integer to the specified stream.
        /// </summary>
        public static void writeVarInt(Stream stream, int paramInt)
        {
            List<byte> data = new List<byte>();
            while ((paramInt & -128) != 0)
            {
                data.Add((byte)(paramInt & 127 | 128));
                paramInt = (int)(((uint)paramInt) >> 7);
            }
            data.Add((byte)paramInt);
            writeBytes(stream, data.ToArray());
        }

        // ====== //
        //  MISC  //
        // ====== //

        /// <summary>
        /// Append several byte arrays
        /// </summary>
        /// <param name="bytes">Bytes to append</param>
        /// <returns>Array containing all the data</returns>
        public static byte[] concatBytes(params byte[][] bytes)
        {
            List<byte> result = new List<byte>();
            foreach (byte[] array in bytes)
                result.AddRange(array);
            return result.ToArray();
        }
    }
}
