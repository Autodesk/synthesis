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
    public class SharedBinaryStream<TStream> where TStream : Stream
    {
        public SharedBinaryStream(TStream stream, ReaderWriterLockSlim lck, int t)
        {
            Timeout = t;
            RWLock = lck;
            Stream = stream;
            Reader = new BinaryReader(Stream);
            Writer = new BinaryWriter(Stream);
        }

        public SharedBinaryStream(TStream stream, ReaderWriterLockSlim lck) : this(stream, lck, DefaultTimeout) { }

        private const int DefaultTimeout = 5000;

        private ReaderWriterLockSlim RWLock { get; set; }

        public int Timeout { get; set; } // ms

        private TStream Stream { get; set; }

        private BinaryReader Reader { get; set; }

        private BinaryWriter Writer { get; set; }

        public void SetStreamPosition(long pos)
        {
            if (RWLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    Stream.Position = pos;
                }
                finally
                {
                    RWLock.ExitReadLock();
                }
            }
            else
            {
                throw new Exception();
            }
        }

        public bool TrySetStreamPosition(long pos)
        {
            if (RWLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    Stream.Position = pos;
                }
                finally
                {
                    RWLock.ExitReadLock();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public byte[] ReadBytes(int count)
        {
            if (RWLock.TryEnterReadLock(Timeout))
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
                    RWLock.ExitReadLock();
                }
            }
            else
            {
                throw new Exception();
            }
        }

        public byte[]? TryReadBytes(int count)
        {
            if (RWLock.TryEnterReadLock(Timeout))
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
                    RWLock.ExitReadLock();
                }
            }
            else
            {
                return null;
            }
        }

        public void WriteBytes(byte[] buffer)
        {
            try
            {
                WriteBytes(buffer, 0, buffer.Length);
            }
            catch
            {
                throw;
            }
        }

        public void WriteBytes(string line)
        {
            try
            {
                WriteBytes(Encoding.UTF8.GetBytes(line));
            }
            catch
            {
                throw;
            }
        }

        public void WriteBytes(byte[] buffer, int index, int count)
        {
            if (RWLock.TryEnterWriteLock(Timeout))
            {
                try
                {
                    Writer.Write(buffer, index, count);
                    Writer.Flush(); // TODO
                }
                finally
                {
                    RWLock.ExitWriteLock();
                }
            }
            else
            {
                throw new Exception();
            }
        }

        public bool TryWriteBytes(string line)
        {
            return TryWriteBytes(Encoding.UTF8.GetBytes(line));
        }

        public bool TryWriteBytes(byte[] buffer)
        {
            return TryWriteBytes(buffer, 0, buffer.Length);
        }

        public bool TryWriteBytes(byte[] buffer, int index, int count)
        {
            if (RWLock.TryEnterWriteLock(Timeout))
            {
                try
                {
                    Writer.Write(buffer, index, count);
                    Writer.Flush(); // TODO
                }
                finally
                {
                    RWLock.ExitWriteLock();
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
