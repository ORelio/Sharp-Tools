using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SharpTools
{
    /// <summary>
    /// Utility class for reading and writing lines directly from/to a socket.
    /// By ORelio - (c) 2014 - Available under the CDDL-1.0 license
    /// </summary>
    public static class SocketExtensions
    {
        /// <summary>
        /// Read line without buffering, as UTF8, directly from socket
        /// </summary>
        /// <param name="socket">Socket to read from</param>
        /// <param name="encoding">Encoding to use (default is UTF-8)</param>
        /// <returns>The line</returns>
        public static string ReadLine(this Socket socket, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            byte[] b = { 0x00 };
            List<byte> readBytes = new List<byte>();
            while (b[0] != '\n')
            {
                if (socket.Receive(b, 1, SocketFlags.None) > 0 && b[0] != '\n')
                    readBytes.Add(b[0]);
            }
            return encoding.GetString(readBytes.ToArray());
        }

        /// <summary>
        /// Write line without buffering, as UTF8, directly to socket
        /// </summary>
        /// <param name="socket">Socket to write to</param>
        /// <param name="line">Line to write to the socket</param>
        /// <param name="encoding">Encoding to use (default is UTF-8)</param>
        /// <returns>The line</returns>
        public static void WriteLine(this Socket socket, string line, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            byte[] toWrite = encoding.GetBytes(line + '\n');
            socket.Send(toWrite, 0, toWrite.Length, SocketFlags.None);
        }
    }
}
