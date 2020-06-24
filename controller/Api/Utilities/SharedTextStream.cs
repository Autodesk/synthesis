using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SynthesisAPI.Utilities
{
    /// <summary>
    /// A thread-dafe text stream
    /// </summary>
    /// <typeparam name="TStream">The type of stream to use</typeparam>
    public class SharedTextStream<TStream> where TStream : Stream
    {
        // TODO ref_count for dispose function?
        // TODO combine with SharedBinaryStream

        public SharedTextStream(TStream stream, ReaderWriterLockSlim lck, int t)
        {
            Timeout = t;
            RWLock = lck;
            Stream = stream;
            Reader = new StreamReader(Stream);
            Writer = new StreamWriter(Stream);
        }

        public SharedTextStream(TStream stream, ReaderWriterLockSlim lck) : this(stream, lck, DefaultTimeout) { }

        private const int DefaultTimeout = 5000;

        private ReaderWriterLockSlim RWLock { get; set; }

        public int Timeout { get; set; } // ms

        public TStream Stream { get; private set; }

        private StreamReader Reader { get; set; }

        private StreamWriter Writer { get; set; }


        public long Seek(long offset, SeekOrigin loc = SeekOrigin.Begin)
        {
            if (RWLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    return Stream.Seek(offset, loc);
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

        public long? TrySeek(long offset, SeekOrigin loc = SeekOrigin.Begin)
        {
            if (RWLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    return Stream.Seek(offset, loc);
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

        public string ReadLine()
        {
            if (RWLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    return Reader.ReadLine();
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

        public string TryReadLine()
        {
            if (RWLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    return Reader.ReadLine();
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

        public string ReadToEnd()
        {
            if (RWLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    return Reader.ReadToEnd();
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

        public string TryReadToEnd()
        {
            if (RWLock.TryEnterReadLock(Timeout))
            {
                try
                {
                    return Reader.ReadToEnd();
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

        public void WriteLine(string line)
        {
            if (RWLock.TryEnterWriteLock(Timeout))
            {
                try
                {
                    Writer.WriteLine(line);
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

        public bool TryWriteLine(string line)
        {
            if (RWLock.TryEnterWriteLock(Timeout))
            {
                try
                {
                    Writer.WriteLine(line);
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
