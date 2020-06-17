using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SynthesisAPI.Utilities
{
    // TODO rename to SharedMemoryStream?
    public class SafeStream : Stream
    {
        public SafeStream(int t)
        {
            Timeout = t;
            RWLock = new ReaderWriterLockSlim();
            Stream = new MemoryStream();
        }

        public SafeStream() : this(5000) { }

        private ReaderWriterLockSlim RWLock { get; set; }

        public int Timeout { get; set; } // ms

        private MemoryStream Stream { get; set; }

        public override bool CanRead => Stream.CanRead;

        public override bool CanSeek => Stream.CanSeek;

        public override bool CanWrite => Stream.CanWrite;

        public override long Length => Stream.Length;

        public override long Position { get => Stream.Position; set => Stream.Position = value; }

        public override void Flush()
        {
            Stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return Stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            Stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (RWLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    return Stream.Read(buffer, offset, count);
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

        public int TryRead(byte[] buffer, int offset, int count)
        {
            if (RWLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    return Stream.Read(buffer, offset, count);
                }
                finally
                {
                    RWLock.ExitReadLock();
                }
            }
            else
            {
                 return 0;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (RWLock.TryEnterWriteLock(Timeout))
            {
                try
                {
                    Stream.Write(buffer, offset, count);
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

        public bool TryWrite(byte[] buffer, int offset, int count)
        {
            if (RWLock.TryEnterWriteLock(Timeout))
            {
                try
                {
                    Stream.Write(buffer, offset, count);
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
