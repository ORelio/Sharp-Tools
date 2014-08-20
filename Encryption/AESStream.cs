using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace SharpTools
{
    /// <summary>
    /// An encrypted stream using AES, used for encrypting or decrypting data on the fly using AES.
    /// Original code found at wiki.vg/Protocol_Encryption without license info, and was removed since that.
    /// </summary>

    public class AesStream : Stream
    {
        CryptoStream enc;
        CryptoStream dec;
        public AesStream(Stream stream, byte[] key, byte[] iv)
        {
            BaseStream = stream;
            enc = new CryptoStream(stream, GenerateAES(key, iv).CreateEncryptor(), CryptoStreamMode.Write);
            dec = new CryptoStream(stream, GenerateAES(key, iv).CreateDecryptor(), CryptoStreamMode.Read);
        }
        public System.IO.Stream BaseStream { get; set; }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int ReadByte()
        {
            return dec.ReadByte();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return dec.Read(buffer, offset, count);
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void WriteByte(byte b)
        {
            enc.WriteByte(b);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            enc.Write(buffer, offset, count);
        }

        private RijndaelManaged GenerateAES(byte[] key, byte[] iv)
        {
            RijndaelManaged cipher = new RijndaelManaged();
            cipher.Padding = PaddingMode.PKCS7;
            cipher.Mode = CipherMode.CBC;
            cipher.KeySize = 128;
            cipher.BlockSize = 128;
            cipher.Key = key;
            cipher.IV = iv;
            return cipher;
        }
    }
}
