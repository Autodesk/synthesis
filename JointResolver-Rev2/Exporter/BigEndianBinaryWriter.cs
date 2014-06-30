// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  BinaryWriter
**
** <owner>[....]</owner>
**
** Purpose: Provides a way to write primitives types in
** binary from a Stream, while also supporting writing Strings
** in a particular encoding.
**
**
===========================================================*/
using System;
using System.Runtime;
using System.Text;
using System.Diagnostics.Contracts;

namespace System.IO
{
    // This abstract base class represents a writer that can write
    // primitives to an arbitrary stream. A subclass can override methods to
    // give unique encodings.
    //
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public class BigEndianBinaryWriter : IDisposable
    {
        public static readonly BigEndianBinaryWriter Null = new BigEndianBinaryWriter();

        protected Stream OutStream;
        private byte[] _buffer;    // temp space for writing primitives to.
        private char[] _tmpOneCharBuffer = new char[1];

        // Size should be around the max number of chars/string * Encoding's max bytes/char
        private const int LargeByteBufferSize = 256;

        // Protected default constructor that sets the output stream
        // to a null stream (a bit bucket).
        protected BigEndianBinaryWriter()
        {
            OutStream = Stream.Null;
            _buffer = new byte[16];
        }

#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public BigEndianBinaryWriter(Stream output)
        {
            if (output == null)
                throw new ArgumentNullException("output");
            if (!output.CanWrite)
                throw new ArgumentException("Not writeable");
            Contract.EndContractBlock();

            OutStream = output;
            _buffer = new byte[16];
        }

        // Closes this writer and releases any system resources associated with the
        // writer. Following a call to Close, any operations on the writer
        // may raise exceptions.
        public virtual void Close()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                OutStream.Close();
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
#if FEATURE_CORECLR
        void IDisposable.Dispose()
#if false // ugly hack to fix syntax for TrimSrc parser, which ignores #if directives
        {
        }
#endif
#else
        public void Dispose()
#endif
        {
            Dispose(true);
        }

        /*
         * Returns the stream associate with the writer. It flushes all pending
         * writes before returning. All subclasses should override Flush to
         * ensure that all buffered data is sent to the stream.
         */
        public virtual Stream BaseStream
        {
            get
            {
                Flush();
                return OutStream;
            }
        }

        // Clears all buffers for this writer and causes any buffered data to be
        // written to the underlying device.
        public virtual void Flush()
        {
            OutStream.Flush();
        }

        public virtual long Seek(int offset, SeekOrigin origin)
        {
            return OutStream.Seek(offset, origin);
        }

        // Writes a boolean to this stream. A single byte is written to the stream
        // with the value 0 representing false or the value 1 representing true.
        //
        public virtual void Write(bool value)
        {
            _buffer[0] = (byte)(value ? 1 : 0);
            OutStream.Write(_buffer, 0, 1);
        }

        // Writes a byte to this stream. The current position of the stream is
        // advanced by one.
        //
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public virtual void Write(byte value)
        {
            OutStream.WriteByte(value);
        }

        // Writes a signed byte to this stream. The current position of the stream
        // is advanced by one.
        //
        public virtual void Write(sbyte value)
        {
            OutStream.WriteByte((byte)value);
        }

        // Writes a byte array to this stream.
        //
        // This default implementation calls the Write(Object, int, int)
        // method to write the byte array.
        //
        public virtual void Write(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            Contract.EndContractBlock();
            OutStream.Write(buffer, 0, buffer.Length);
        }

        // Writes a section of a byte array to this stream.
        //
        // This default implementation calls the Write(Object, int, int)
        // method to write the byte array.
        //
        public virtual void Write(byte[] buffer, int index, int count)
        {
            OutStream.Write(buffer, index, count);
        }


        // Writes a double to this stream. The current position of the stream is
        // advanced by eight.
        //
        [System.Security.SecuritySafeCritical]  // auto-generated
        public unsafe virtual void Write(double value)
        {
            ulong TmpValue = *(ulong*)&value;
            _buffer[7] = (byte)TmpValue;
            _buffer[6] = (byte)(TmpValue >> 8);
            _buffer[5] = (byte)(TmpValue >> 16);
            _buffer[4] = (byte)(TmpValue >> 24);
            _buffer[3] = (byte)(TmpValue >> 32);
            _buffer[2] = (byte)(TmpValue >> 40);
            _buffer[1] = (byte)(TmpValue >> 48);
            _buffer[0] = (byte)(TmpValue >> 56);
            OutStream.Write(_buffer, 0, 8);
        }

        // Writes a two-byte signed integer to this stream. The current position of
        // the stream is advanced by two.
        //
        public virtual void Write(short value)
        {
            _buffer[1] = (byte)value;
            _buffer[0] = (byte)(value >> 8);
            OutStream.Write(_buffer, 0, 2);
        }

        // Writes a two-byte unsigned integer to this stream. The current position
        // of the stream is advanced by two.
        //
        public virtual void Write(ushort value)
        {
            _buffer[1] = (byte)value;
            _buffer[0] = (byte)(value >> 8);
            OutStream.Write(_buffer, 0, 2);
        }

        // Writes a four-byte signed integer to this stream. The current position
        // of the stream is advanced by four.
        //
        public virtual void Write(int value)
        {
            _buffer[3] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _buffer[1] = (byte)(value >> 16);
            _buffer[0] = (byte)(value >> 24);
            OutStream.Write(_buffer, 0, 4);
        }

        // Writes a four-byte unsigned integer to this stream. The current position
        // of the stream is advanced by four.
        //

        public virtual void Write(uint value)
        {
            _buffer[3] = (byte)value;
            _buffer[2] = (byte)(value >> 8);
            _buffer[1] = (byte)(value >> 16);
            _buffer[0] = (byte)(value >> 24);
            OutStream.Write(_buffer, 0, 4);
        }

        // Writes an eight-byte signed integer to this stream. The current position
        // of the stream is advanced by eight.
        //
        public virtual void Write(long value)
        {
            _buffer[7] = (byte)value;
            _buffer[6] = (byte)(value >> 8);
            _buffer[5] = (byte)(value >> 16);
            _buffer[4] = (byte)(value >> 24);
            _buffer[3] = (byte)(value >> 32);
            _buffer[2] = (byte)(value >> 40);
            _buffer[1] = (byte)(value >> 48);
            _buffer[0] = (byte)(value >> 56);
            OutStream.Write(_buffer, 0, 8);
        }

        // Writes an eight-byte unsigned integer to this stream. The current
        // position of the stream is advanced by eight.
        //
        public virtual void Write(ulong value)
        {
            _buffer[7] = (byte)value;
            _buffer[6] = (byte)(value >> 8);
            _buffer[5] = (byte)(value >> 16);
            _buffer[4] = (byte)(value >> 24);
            _buffer[3] = (byte)(value >> 32);
            _buffer[2] = (byte)(value >> 40);
            _buffer[1] = (byte)(value >> 48);
            _buffer[0] = (byte)(value >> 56);
            OutStream.Write(_buffer, 0, 8);
        }

        // Writes a float to this stream. The current position of the stream is
        // advanced by four.
        //
        [System.Security.SecuritySafeCritical]  // auto-generated
        public unsafe virtual void Write(float value)
        {
            uint TmpValue = *(uint*)&value;
            _buffer[3] = (byte)TmpValue;
            _buffer[2] = (byte)(TmpValue >> 8);
            _buffer[1] = (byte)(TmpValue >> 16);
            _buffer[0] = (byte)(TmpValue >> 24);
            OutStream.Write(_buffer, 0, 4);
        }
    }
}