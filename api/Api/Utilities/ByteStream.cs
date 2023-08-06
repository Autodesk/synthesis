using System;
using System.Collections.Generic;
using System.IO;

namespace SynthesisAPI.Utilities {

    /// <summary>
    /// Creates a read-only stream from a byte enumerator
    /// </summary>
    public class ByteStream : Stream {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;

        private readonly long _length;
        public override long Length => _length;

        private long _alreadyRead = 0;
        public override long Position { get => _alreadyRead; set => throw new System.NotImplementedException(); }

        private readonly IEnumerator<byte> _enumer;

        /// <summary>
        /// Constructs a stream from a byte enumerator
        /// </summary>
        /// <param name="enumer">Enumerator or byte data</param>
        /// <param name="length">Length of byte data</param>
        public ByteStream(IEnumerator<byte> enumer, long length) {
            _enumer = enumer;
            _length = length;
        }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count) {
            int position = offset;
            while (position < buffer.Length && position < offset + count && _enumer.MoveNext()) {
                buffer[position] = _enumer.Current;
                _alreadyRead++;
                position++;
            }
            return position - offset;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }
        public override void SetLength(long value) {
            throw new NotImplementedException();
        }
        public override void Write(byte[] buffer, int offset, int count) { 
            throw new NotImplementedException();
        }
    }

}