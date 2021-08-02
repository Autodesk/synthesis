using System.IO;
using System.IO.Compression;

namespace Assets.Scripts.Engine.Util
{
	/// <summary>
	/// Wrapper class for <see cref="System.IO.Compression.DeflateStream"/>
	/// 
	/// <see cref="DeflateStream"/> does not support the <see cref="Stream.Length"/> property, 
	/// so this wrapper wraps the stream and allows a stream length to be supplied in the constructor
	/// </summary>
    internal class DeflateStreamWrapper : Stream
	{
		private Stream inner;
		private long? length;

		public DeflateStreamWrapper(Stream stream, long? len = null)
		{
			// if(stream is DeflateStream) // TODO figure this out
			inner = stream;
			length = len;
		}

		public override bool CanRead => inner.CanRead;

		public override bool CanSeek => inner.CanSeek;

		public override bool CanWrite => inner.CanWrite;

		public override long Length => length != null ? length.Value : inner.Length;

		public override long Position { get => inner.Position; set => inner.Position = value; }

		public override void Flush() => inner.Flush();

		public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);

		public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

		public override void SetLength(long value) => inner.SetLength(value);

		public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);

		public override void Close() => inner.Close();
	}
}
