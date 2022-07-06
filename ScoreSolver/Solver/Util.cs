using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ScoreSolver
{
    class Util
    {
        /// <summary>
        /// Get number of buttons in bitmask
        /// </summary>
        public static uint CountButtons(ButtonState buttons)
        {
            UInt32 v = (UInt32)buttons;
            v = v - ((v >> 1) & 0x55555555); // reuse input as temporary
            v = (v & 0x33333333) + ((v >> 2) & 0x33333333); // temp
            UInt32 c = ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
            return c;
        }

        /// <summary>
        /// Translate button bitmask into something user-readable
        /// </summary>
        public static string ButtonsToString(ButtonState buttons)
        {
            if (buttons == ButtonState.None) return "NONE";

            StringBuilder b = new StringBuilder(4);
            
            if (buttons.HasFlag(ButtonState.Triangle)) b.Append("T");
            else b.Append(" ");
            if (buttons.HasFlag(ButtonState.Square)) b.Append("S");
            else b.Append(" ");
            if (buttons.HasFlag(ButtonState.Cross)) b.Append("X");
            else b.Append(" ");
            if (buttons.HasFlag(ButtonState.Circle)) b.Append("O");
            else b.Append(" ");

            return b.ToString();
        }
    }

    static class MaxByExtz
    {
        /// <summary>
        /// Find an item with the max value of provided lambda function
        /// </summary>
        public static T MaxBy<T, R>(this IEnumerable<T> en, Func<T, R> evaluate) where R : IComparable<R>
        {
            if (en.Count() < 1) return default(T);
            return en.Select(t => new Tuple<T, R>(t, evaluate(t)))
                .Aggregate((max, next) => next.Item2.CompareTo(max.Item2) > 0 ? next : max).Item1;
        }
    }

    static class SocketExtz
    {
        /// <summary>
        /// Compress the bytes with GZip
        /// </summary>
        private static byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        /// <summary>
        /// Decompress GZip data
        /// </summary>
        private static byte[] Decompress(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                zipStream.Close();
                return resultStream.ToArray();
            }
        }

        /// <summary>
        /// Send an object to be received with <see cref="ReceiveObject"/> on the other side
        /// </summary>
        public static void SendObject(this Socket me, object obj)
        {
            if (obj != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    (new BinaryFormatter()).Serialize(memoryStream, obj);
                    byte[] wkData = memoryStream.ToArray();
                    wkData = Compress(wkData);
                    var len = BitConverter.GetBytes((UInt32)wkData.Length);
                    me.Send(len);
                    me.Send(wkData);
                }
            }
            else
            {
                var len = BitConverter.GetBytes((UInt32)0);
                me.Send(len);
            }
        }

        /// <summary>
        /// Receive an object sent by <see cref="SendObject"/>
        /// </summary>
        public static T ReceiveObject<T>(this Socket client)
        {
            byte[] len = new byte[4];
            var size = client.Receive(len,4,SocketFlags.None);
            if (size == 4)
            {
                var expectLen = BitConverter.ToUInt32(len, 0);
                if (expectLen == 0)
                {
                    return default(T);
                }

                var remainLen = expectLen;
                byte[] wkData = new byte[expectLen];
                while (remainLen > 0)
                {
                    var recvLen = client.Receive(wkData, (int)(expectLen - remainLen), (int)remainLen, SocketFlags.None);
                    if (recvLen < 0)
                    {
                        break;
                    }
                    remainLen -= (uint)recvLen;
                }

                if (remainLen == 0)
                {
                    wkData = Decompress(wkData);
                    object wk = null;
                    using (var memoryStream = new MemoryStream(wkData))
                        wk = (T)(new BinaryFormatter()).Deserialize(memoryStream);
                    if(wk != null)
                    {
                        return (T)wk;
                    }
                }
                return default(T);
            }
            else return default(T);
        }

        /// <summary>
        /// Send an object to be received with <see cref="ReceiveObject"/> on the other side
        /// </summary>
        public static void SendObject(this TcpClient me, object obj)
        {
            if (obj != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    (new BinaryFormatter()).Serialize(memoryStream, obj);
                    byte[] wkData = memoryStream.ToArray();
                    wkData = Compress(wkData);
                    var s = me.GetStream();
                    var len = BitConverter.GetBytes((UInt32)wkData.Length);
                    s.Write(len, 0, len.Length);
                    s.Flush();
                    s.Write(wkData, 0, wkData.Length);
                    s.Flush();
                }
            }
            else
            {
                var s = me.GetStream();
                var len = BitConverter.GetBytes((UInt32) 0);
                s.Write(len, 0, len.Length);
                s.Flush(); 
            }
        }

        /// <summary>
        /// Receive an object sent by <see cref="SendObject"/>
        /// </summary>
        public static T ReceiveObject<T>(this TcpClient client)
        {
            byte[] len = new byte[4];
            var stream = client.GetStream();
            var size = 0;
            while(size < len.Length)
            {
                var read = stream.Read(len, size, len.Length - size);
                if (read < 0) return default(T);
                size += read;
            }

            if (size == 4)
            {
                var expectLen = BitConverter.ToUInt32(len, 0);
                if (expectLen == 0)
                {
                    return default(T);
                }

                var remainLen = expectLen;
                byte[] wkData = new byte[expectLen];
                while (remainLen > 0)
                {
                    var recvLen = stream.Read(wkData, (int)(expectLen - remainLen), (int)remainLen);
                    if (recvLen < 0)
                    {
                        break;
                    }
                    remainLen -= (uint)recvLen;
                }

                if (remainLen == 0)
                {
                    wkData = Decompress(wkData);
                    object wk = null;
                    using (var memoryStream = new MemoryStream(wkData))
                        wk = (T)(new BinaryFormatter()).Deserialize(memoryStream);
                    if (wk != null)
                    {
                        return (T)wk;
                    }
                }
                return default(T);
            }
            else return default(T);
        }
    }
}
