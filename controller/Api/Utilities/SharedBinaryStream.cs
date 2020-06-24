using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace SynthesisAPI.Utilities
{
    /// <summary>
    /// A thread-dafe binary stream
    /// </summary>
    /// <typeparam name="TStream">The type of stream to use</typeparam>
    public class SharedBinaryStream
    {
        public SharedBinaryStream(Stream stream, ReaderWriterLockSlim lck, int t)
        {
            Timeout = t;
            RwLock = lck;
            Stream = stream;
            Reader = new BinaryReader(Stream);
            Writer = new BinaryWriter(Stream);
        }

        public SharedBinaryStream(Stream stream, ReaderWriterLockSlim lck) : this(stream, lck, DefaultTimeout) { }

        private const int DefaultTimeout = 5000;

        private ReaderWriterLockSlim RwLock { get; set; }

        public int Timeout { get; set; } // ms

        public Stream Stream { get; private set; }

        private BinaryReader Reader { get; set; }

        private BinaryWriter Writer { get; set; }

        public long Seek(long offset, SeekOrigin loc = SeekOrigin.Begin)
        {
            if (RwLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    return Stream.Seek(offset, loc);
                }
                finally
                {
                    RwLock.ExitReadLock();
                }
            }
            else
            {
                throw new Exception();
            }
            
        }

        public long? TrySeek(long offset, SeekOrigin loc = SeekOrigin.Begin)
        {
            if (RwLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    return Stream.Seek(offset, loc);
                }
                finally
                {
                    RwLock.ExitReadLock();
                }
            }
            else
            {
                return null;
            }

        }

        public byte[] ReadBytes(int count)
        {
            if (RwLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    long pos = Stream.Position;
                    byte[] read = Reader.ReadBytes(count);
                    Stream.Position = pos + read.Length;
                    return read;
                }
                finally
                {
                    RwLock.ExitReadLock();
                }
            }
            else
            {
                throw new Exception();
            }
        }

        public byte[]? TryReadBytes(int count)
        {
            if (RwLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    long pos = Stream.Position;
                    byte[] read = Reader.ReadBytes(count);
                    Stream.Position = pos + read.Length;
                    return read;
                }
                finally
                {
                    RwLock.ExitReadLock();
                }
            }
            else
            {
                return null;
            }
        }

        public byte[] ReadToEnd()
        {
            return ReadBytes((int)Stream.Length);
        }

        public byte[]? TryReadToEnd()
        {
            return TryReadBytes((int)Stream.Length);
        }

        public void WriteBytes(byte[] buffer) => WriteBytes(buffer, 0, buffer.Length);

        public void WriteBytes(string line) => WriteBytes(Encoding.UTF8.GetBytes(line));
        public void WriteBytes(byte[] buffer, int index, int count)
        {
            if (RwLock.TryEnterWriteLock(Timeout))
            {
                try
                {
                    Writer.Write(buffer, index, count);
                    Writer.Flush(); // TODO
                }
                finally
                {
                    RwLock.ExitWriteLock();
                }
            }
            else
            {
                throw new Exception();
            }
        }

        public bool TryWriteBytes(string line) =>
            TryWriteBytes(Encoding.UTF8.GetBytes(line));


        public bool TryWriteBytes(byte[] buffer) =>
            TryWriteBytes(buffer, 0, buffer.Length);

        public bool TryWriteBytes(byte[] buffer, int index, int count)
        {
            if (RwLock.TryEnterWriteLock(Timeout))
            {
                try
                {
                    Writer.Write(buffer, index, count);
                    Writer.Flush(); // TODO
                }
                finally
                {
                    RwLock.ExitWriteLock();
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
