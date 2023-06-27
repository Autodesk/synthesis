using System;
using System.IO;
using System.Threading;

#nullable enable

namespace SynthesisAPI.Utilities
{
    /// <summary>
    /// A thread-dafe text stream
    /// </summary>
    /// <typeparam name="TStream">The type of stream to use</typeparam>
    public class SharedTextStream
    {
        // TODO ref_count for dispose function?
        // TODO combine with SharedBinaryStream

        public SharedTextStream(Stream stream, ReaderWriterLockSlim lck, int t)
        {
            Timeout = t;
            RwLock = lck;
            Stream = stream;
            Reader = new StreamReader(Stream);
            Writer = new StreamWriter(Stream);
        }

        public SharedTextStream(Stream stream, ReaderWriterLockSlim lck) : this(stream, lck, DefaultTimeout) { }

        private const int DefaultTimeout = 5000;

        private ReaderWriterLockSlim RwLock { get; set; }

        public int Timeout { get; set; } // ms

        public Stream Stream { get; private set; }

        private StreamReader Reader { get; set; }

        private StreamWriter Writer { get; set; }

        public void SetLength(long len)
        {
            if (RwLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    Stream.SetLength(len);
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

        public bool TrySetLength(long len)
        {
            if (RwLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    Stream.SetLength(len);
                }
                finally
                {
                    RwLock.ExitReadLock();
                }
                return true;
            }
            else
            {
                return false;
            }

        }
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

        public string ReadLine()
        {
            if (RwLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    return Reader.ReadLine();
                }
                finally
                {
                    RwLock.ExitReadLock();
                }
            }
            else
            {
                throw new Exception("Cannot ReadLine, Thread lock timed out"); // why though and also what, should this be an empty string ?, probably should through a specific exception about the mutex lock
            }
        }

        public string? TryReadLine()
        {
            if (RwLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    return Reader.ReadLine();
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

        public string ReadToEnd()
        {
            if (RwLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    return Reader.ReadToEnd();
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

        public string? TryReadToEnd()
        {
            if (RwLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    return Reader.ReadToEnd();
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

        public void WriteLine(string line)
        {
            if (RwLock.TryEnterWriteLock(Timeout))
            {
                try
                {
                    Writer.WriteLine(line);
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

        public bool TryWriteLine(string line)
        {
            if (RwLock.TryEnterWriteLock(Timeout))
            {
                try
                {
                    Writer.WriteLine(line);
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
