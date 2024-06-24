// <copyright file="ProxyStream.cs" company="Eötvös Loránd University (ELTE)">
//     Copyright 2016-2021 Roberto Giachetta. Licensed under the
//     Educational Community License, Version 2.0 (the "License"); you may
//     not use this file except in compliance with the License. You may
//     obtain a copy of the License at
//     http://opensource.org/licenses/ECL-2.0
// 
//     Unless required by applicable law or agreed to in writing,
//     software distributed under the License is distributed on an "AS IS"
//     BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
//     or implied. See the License for the specific language governing
//     permissions and limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AEGIS.IO.Utilities
{
    /// <summary>
    /// Works as a proxy for an underlying stream.
    /// </summary>
    public class ProxyStream : Stream
    {
        #region Constant fields

        /// <summary>
        /// The size of the buffer arrays used to cache data. This field is constant.
        /// </summary>
        private const Int32 DefaultBufferSize = 1 << 16; // (64 KB)

        private readonly Byte[] EmptyBuffer;

        #endregion

        #region Private types

        /// <summary>
        /// Defines the access types of the stream.
        /// </summary>
        [Flags]
        private enum StreamAccessType
        {
            /// <summary>
            /// Indicates that the access type is not defined.
            /// </summary>
            Undefined = 0,

            /// <summary>
            /// Indicates that the stream is readable.
            /// </summary>
            Readable = 1,

            /// <summary>
            /// Indicates that the stream is writable.
            /// </summary>
            Writable = 2
        };

        /// <summary>
        /// Defines the modes for updating data flags.
        /// </summary>
        private enum UpdateMode
        {
            /// <summary>
            /// Indicates that the flags should be added.
            /// </summary>
            Add,

            /// <summary>
            /// Indicates that the flags should be removed.
            /// </summary>
            Remove
        }

        #endregion

        #region Private fields

        /// <summary>
        /// The underlying stream.
        /// </summary>
        private Stream _baseStream;

        /// <summary>
        /// Defines whether the instances has been disposed or not. 
        /// </summary>
        private Boolean _disposed;

        /// <summary>
        /// A value indicating whether the bytes can be read or written multiple times or not. 
        /// </summary>
        private Boolean _isSingleAccess;

        /// <summary>
        /// A value indicating whether proxy mode is forced.
        /// </summary>
        private Boolean _isProxyModeForced;
        
        /// <summary>
        /// The current position in the stream.
        /// </summary>
        private Int64 _position;

        /// <summary>
        /// The length of the stream.
        /// </summary>
        private Int64 _length;
        
        /// <summary>
        /// The access type of the stream.
        /// </summary>
        private StreamAccessType _accessType;

        /// <summary>
        /// The bit flag arrays.
        /// </summary>
        private Dictionary<Int32, Byte[]> _bitFlags;

        /// <summary>
        /// The data containing arrays.
        /// </summary>
        private Dictionary<Int32, Byte[]> _buffers;

        /// <summary>
        /// A value indicating whether to dispose the underlying stream.
        /// </summary>
        private Boolean _disposeBaseStream;

        /// <summary>
        /// The number of bytes stored in the buffers.
        /// </summary>
        private Int32 _bufferSize;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyStream" /> class.
        /// </summary>
        /// <param name="stream">The underlying stream.</param>
        /// <exception cref="System.ArgumentNullException">The stream is null.</exception>
        /// <exception cref="System.NotSupportedException">The stream does not support reading and writing.</exception>
        public ProxyStream(Stream stream)
            : this(stream, false, ProxyStreamOptions.Default, DefaultBufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyStream" /> class.
        /// </summary>
        /// <param name="stream">The underlying stream.</param>
        /// <param name="options">The proxy stream options.</param>
        /// <exception cref="System.ArgumentNullException">The stream is null.</exception>
        /// <exception cref="System.NotSupportedException">The stream does not support reading and writing.</exception>
        public ProxyStream(Stream stream, ProxyStreamOptions options)
            : this(stream, false, options, DefaultBufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyStream" /> class.
        /// </summary>
        /// <param name="stream">The underlying stream.</param>
        /// <param name="disposeStream">A value indicating whether to dispose the underlying stream.</param>
        /// <param name="options">The proxy stream options.</param>
        /// <exception cref="System.ArgumentNullException">The stream is null.</exception>
        /// <exception cref="System.NotSupportedException">The stream does not support reading and writing.</exception>
        public ProxyStream(Stream stream, Boolean disposeStream, ProxyStreamOptions options)
            : this(stream, disposeStream, options, DefaultBufferSize)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyStream" /> class.
        /// </summary>
        /// <param name="underlyingStream">The underlying stream.</param>
        /// <param name="disposeUnderlyingStream">A value indicating whether to dispose the underlying stream.</param>
        /// <param name="options">The proxy stream options.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">The buffer size is less than 1.</exception>
        /// <exception cref="System.ArgumentNullException">The stream is null.</exception>
        /// <exception cref="System.NotSupportedException">The stream does not support reading and writing.</exception>
        public ProxyStream(Stream underlyingStream, Boolean disposeUnderlyingStream, ProxyStreamOptions options, Int32 bufferSize)
        {
            if (underlyingStream == null)
                throw new ArgumentNullException("stream", "The stream is null.");

            if (!underlyingStream.CanRead && !underlyingStream.CanWrite)
                throw new NotSupportedException("The stream does not support reading and writing.");

            if (bufferSize < 1)
                throw new ArgumentOutOfRangeException("bufferSize", "The buffer size is less than 1.");
        
            _baseStream = underlyingStream;
            _isSingleAccess = options.HasFlag(ProxyStreamOptions.SingleAccess);
            _isProxyModeForced = options.HasFlag(ProxyStreamOptions.ForceProxy);
            _bufferSize = bufferSize;

            if (_baseStream.CanWrite && !_baseStream.CanRead)
                InitializeStream(StreamAccessType.Writable);
            else if (!_baseStream.CanWrite && _baseStream.CanRead)
                InitializeStream(StreamAccessType.Readable);

            _disposed = false;
            _disposeBaseStream = disposeUnderlyingStream;

            EmptyBuffer = new Byte[_bufferSize];
        }

        #endregion

        #region Stream properties

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override Int64 Position
        {
            get
            {
                return _position;
            }
            set
            {
                if (_position != value)
                {
                    Seek(_position, SeekOrigin.Begin);
                }
            }
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override Int64 Length { get { return _length; } }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override Boolean CanSeek
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override Boolean CanWrite
        {
            get { return (_accessType == StreamAccessType.Writable || _accessType == StreamAccessType.Undefined) ? true : false; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override Boolean CanRead
        {
            get { return (_accessType == StreamAccessType.Readable || _accessType == StreamAccessType.Undefined) ? true : false; }
        }

        #endregion

        #region Stream methods

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <exception cref="System.ObjectDisposedException">Method was called after the stream was closed.</exception>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Int64 Seek(Int64 offset, SeekOrigin origin)
        {
            if (_disposed)
                throw new ObjectDisposedException("Method was called after the stream was closed.");

            if (!_isProxyModeForced && _baseStream.CanSeek)
                return _baseStream.Seek(offset, origin);
            
            switch (origin)
            {
                case SeekOrigin.Begin:
                    _position = offset;
                    break;
                case SeekOrigin.Current:
                    _position += offset;
                    break;
                case SeekOrigin.End:
                    _position = offset + _baseStream.Length;
                    break;
            }

            return _position;
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream, stores them in the cache and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer.</returns>
        /// <exception cref="System.ObjectDisposedException">Method was called after the stream was closed.</exception>
        /// <exception cref="System.ArgumentNullException">The buffer is null.</exception>
        /// <exception cref="System.ArgumentException">The sum of offset and count is larger than the buffer length.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The offset or count is negative.</exception>
        /// <exception cref="System.NotSupportedException">The stream does not support reading.</exception>
        public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
        {
            if (_disposed)
                throw new ObjectDisposedException("Method was called after the stream was closed.");

            if (buffer == null)
                throw new ArgumentNullException("The buffer is null.");

            if (offset + count > buffer.Length)
                throw new ArgumentException("The sum of offset and count is larger than the buffer length.");

            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException("The offset or count is negative.");

            // check whether data can be directly read
            if (!_isProxyModeForced && _baseStream.CanSeek && _baseStream.CanRead)
            {
                // read the data from the source stream
                return _baseStream.Read(buffer, offset, count);
            }

            InitializeStream(StreamAccessType.Readable);

            if (_accessType != StreamAccessType.Readable)
                throw new NotSupportedException("The stream does not support reading.");

            if (_position >= _length)
                return 0;

            if (!IsBufferAvailable(_position, Math.Min(count, _baseStream.Position - _position)))
                throw new InvalidOperationException("The specified data was already read.");

            // check whether data needs to be read from the underlying stream
            if (_position + count > _baseStream.Position)
            {
                try
                {
                    ReadDataFromStream(_position + count - _baseStream.Position);
                }
                catch (Exception ex)
                {
                    throw new IOException("Error occurred during underlying stream reading.", ex);
                }
            }

            return ReadDataFromBuffer(buffer, offset, count);
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="System.ObjectDisposedException">Method was called after the stream was closed.</exception>
        /// <exception cref="System.ArgumentNullException">The buffer is null</exception>
        /// <exception cref="System.ArgumentException">The sum of offset and count is larger than the buffer length.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The offset or count is negative.</exception>
        /// <exception cref="System.NotSupportedException">The stream does not support writing.</exception>
        public override void Write(Byte[] buffer, Int32 offset, Int32 count)
        {
            if (_disposed)
                throw new ObjectDisposedException("Method was called after the stream was closed.");

            if (!_isProxyModeForced && _baseStream.CanSeek)
            {
                _baseStream.Write(buffer, offset, count);
                return;
            }

            if (buffer == null)
                throw new ArgumentNullException("The buffer is null");

            if (offset + count > buffer.Length)
                throw new ArgumentException("The sum of offset and count is larger than the buffer length.");

            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException("The offset or count is negative.");

            InitializeStream(StreamAccessType.Writable);

            if (_accessType != StreamAccessType.Writable)
                throw new NotSupportedException("The stream does not support writing.");

            if (!IsBufferAvailable(_position, Math.Min(count, _baseStream.Position - _position)))
                throw new InvalidOperationException("Data was already written at the specified position.");

            WriteDataIntoBuffer(buffer, offset, count);
            
            WriteDataIntoStream(false);
        }

        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="System.ObjectDisposedException">Method was called after the stream was closed.</exception>
        /// <exception cref="System.InvalidOperationException">The stream is not writable or does not support seeking.</exception>
        public override void SetLength(Int64 value)
        {
            if (_disposed)
                throw new ObjectDisposedException("Method was called after the stream was closed.");

            if (_accessType == StreamAccessType.Readable)
                throw new InvalidOperationException("The stream is not writable.");

            if (_accessType == StreamAccessType.Undefined)
                _accessType = StreamAccessType.Writable;

            if (_length < value)
            {
                CreateBuffer(_length, value - _length);
            }

            if (_length > value)
            {
                RemoveBuffer(value, _length - value);
            }

            _length = value;
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying stream.
        /// </summary>
        public override void Flush()
        {
            if (_accessType != StreamAccessType.Writable)
                return;

            WriteDataIntoStream(true);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gets a value indicating whether the buffers are available.
        /// </summary>
        /// <param name="offset">The offset of the first byte with respect to the start of the stream.</param>
        /// <param name="count">The number of bytes.</param>
        /// <returns><c>true</c> if the specified data is already disposed; otherwise, <c>false</c>.</returns>
        private Boolean IsBufferAvailable(Int64 offset, Int64 count)
        {
            Int32 bufferIndex = (Int32)(offset / _bufferSize);
            while (count > 0)
            {
                if (!_buffers.ContainsKey(bufferIndex))
                    return false;

                count -= _bufferSize;
                bufferIndex++;
            }
            
            return true;
        }

        private Boolean IsBufferFilled(Int32 bufferIndex)
        {
            if (!_isSingleAccess)
                return false;

            return _bitFlags != null && _bitFlags.ContainsKey(bufferIndex) && _bitFlags[bufferIndex].All(value => value == Byte.MaxValue);
        }

        /// <summary>
        /// Reads the necessary bytes from the underlying stream and stores them in the buffers.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        private void ReadDataFromStream(Int64 count)
        {           
            Int32 bufferIndex = (Int32)(_baseStream.Position / _bufferSize);
            
            while (count > 0)
            {
                // read content
                Byte[] bytes = new Byte[_bufferSize];

                Int32 numberOfBytesRead = _baseStream.Read(bytes, 0, bytes.Length);

                _buffers.Add(bufferIndex, bytes);
                UpdateFlags(bufferIndex, 0, Math.Min(numberOfBytesRead, bytes.Length), UpdateMode.Add);

                if (numberOfBytesRead < bytes.Length)
                    return;

                count -= numberOfBytesRead;

                bufferIndex++;
            }
        }

        /// <summary>
        /// Reads the data from the buffers.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read from the buffers.</returns>
        private Int32 ReadDataFromBuffer(Byte[] buffer, Int32 offset, Int32 count)
        {
            Int32 bufferIndex = (Int32)(_position / _bufferSize);

            Int32 numberOfBytesRead = 0;
            Int32 readOffset = (Int32)(_position % _bufferSize);
            Int32 readCount = Math.Min(count, _bufferSize - readOffset);

            while (numberOfBytesRead < count)
            {
                if (!_buffers.ContainsKey(bufferIndex))
                    return numberOfBytesRead;

                Array.Copy(_buffers[bufferIndex], readOffset, buffer, offset, readCount);

                UpdateFlags(bufferIndex, readOffset, readCount, UpdateMode.Remove);
                RemoveBuffer(bufferIndex);

                numberOfBytesRead += readCount;
                offset += readCount;
                readOffset = 0;
                readCount = Math.Min(count - numberOfBytesRead, _bufferSize);

                bufferIndex++;
            }

            _position += numberOfBytesRead;
            return numberOfBytesRead;
        }

        /// <summary>
        /// Writes the data into the buffers.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        private void WriteDataIntoBuffer(Byte[] buffer, Int32 offset, Int32 count)
        {
            Int32 bufferIndex = (Int32)(_position / _bufferSize);

            Int64 numberOfBytesWritten = 0;
            Int32 writeOffset = (Int32)(_position % _bufferSize);
            Int32 writeCount = Math.Min(count, _bufferSize - writeOffset);

            while (numberOfBytesWritten < count)
            {
                CreateBuffer(bufferIndex);

                Array.Copy(buffer, offset, _buffers[bufferIndex], writeOffset, writeCount);

                UpdateFlags(bufferIndex, writeOffset, writeCount, UpdateMode.Add);

                numberOfBytesWritten += writeCount;
                offset += writeCount;
                writeOffset = 0;
                writeCount = (Int32)Math.Min(count - numberOfBytesWritten, _bufferSize);

                bufferIndex++;
            }

            _position += numberOfBytesWritten;

            if (_length < _position)
                _length = _position;
        }

        /// <summary>
        /// Writes the data into the stream.
        /// </summary>
        /// <param name="forced">A value indicating whether the writing is forced.</param>
        private void WriteDataIntoStream(Boolean forced)
        {
            if (!_isSingleAccess && !forced)
                return;

            Int32 bufferIndex = (Int32)(_baseStream.Position / _bufferSize);

            Int64 numberOfBytesToWrite = _length - _baseStream.Position;
            Int32 writeOffset = (Int32)(_baseStream.Position % _bufferSize);
            Int32 writeCount = (Int32)Math.Min(numberOfBytesToWrite, _bufferSize - writeOffset);

            while (numberOfBytesToWrite > 0)
            {
                if (!_buffers.ContainsKey(bufferIndex))
                {
                    if (!forced)
                        break;

                    _baseStream.Write(EmptyBuffer, writeOffset, writeCount);
                }
                else
                {
                    if (!forced && !IsBufferFilled(bufferIndex))
                        break;

                    _baseStream.Write(_buffers[bufferIndex], writeOffset, writeCount);

                    UpdateFlags(bufferIndex, writeOffset, writeCount, UpdateMode.Remove);
                    RemoveBuffer(bufferIndex);
                }

                numberOfBytesToWrite -= writeCount;
                writeOffset = 0;
                writeCount = (Int32)Math.Min(numberOfBytesToWrite, _bufferSize);

                bufferIndex++;
            }
        }

        /// <summary>
        /// Updates the data flags with the specified availability.
        /// </summary>
        /// <param name="bufferIndex">The index of the buffer in which the update is performed.</param>
        /// <param name="offset">The offset of the first byte within the specified buffer.</param>
        /// <param name="count">The number of bytes within the buffer.</param>
        /// <param name="mode">The flag update mode.</param>
        private void UpdateFlags(Int32 bufferIndex, Int32 offset, Int32 count, UpdateMode mode)
        {
            if (!_isSingleAccess)
                return;

            if (!_bitFlags.ContainsKey(bufferIndex))
            {
                if (mode == UpdateMode.Add)
                {
                    _bitFlags.Add(bufferIndex, new Byte[_bufferSize / 8]);
                }
                else
                {
                    return;
                }
            }

            Int32 byteIndex = offset / 8;

            Int32 numberOfBitsSet = 0;
            Int32 bitOffset = offset % 8;
            Int32 bitCount = Math.Min(count, 8 - bitOffset);

            while (numberOfBitsSet < count)
            {
                switch (mode)
                {
                    case UpdateMode.Add:
                        _bitFlags[bufferIndex][byteIndex] |= GetBitMask(bitOffset, bitCount);
                        break;
                    case UpdateMode.Remove:
                        _bitFlags[bufferIndex][byteIndex] ^= GetBitMask(bitOffset, bitCount);
                        break;
                }

                byteIndex++;
                numberOfBitsSet += bitCount;

                bitOffset = 0;
                bitCount = Math.Min(count - numberOfBitsSet, 8);
            }
        }

        /// <summary>
        /// Returns the bit mask for the specified offset and count.
        /// </summary>
        /// <param name="offset">The offset of the first byte.</param>
        /// <param name="count">The number of bytes.</param>
        /// <returns>The bit mask used for updating the data flags.</returns>
        private Byte GetBitMask(Int32 offset, Int32 count)
        {
            return (Byte)(256 - (1 << (8 - count)) >> offset);
        }

        /// <summary>
        /// Creates buffer arrays.
        /// </summary>
        /// <param name="offset">The offset of the first byte.</param>
        /// <param name="count">The number of bytes.</param>
        private void CreateBuffer(Int64 offset, Int64 count)
        {
            Int32 startBufferIndex = (Int32)(offset / _bufferSize);
            Int32 endBufferIndex = (Int32)((offset + count) / _bufferSize);

            for (Int32 bufferIndex = startBufferIndex; bufferIndex <= endBufferIndex; bufferIndex++)
            {
                CreateBuffer(bufferIndex);
            }
        }

        /// <summary>
        /// Creates a buffer array.
        /// </summary>
        /// <param name="bufferIndex">The index of the buffer.</param>
        private void CreateBuffer(Int32 bufferIndex)
        {
            if (_buffers.ContainsKey(bufferIndex))
                return;

            _buffers.Add(bufferIndex, new Byte[_bufferSize]);

            if (_isSingleAccess)
                _bitFlags.Add(bufferIndex, new Byte[_bufferSize / 8]);
        }

        /// <summary>
        /// Removes buffer arrays.
        /// </summary>
        /// <param name="offset">The offset of the first byte.</param>
        /// <param name="count">The number of bytes.</param>
        private void RemoveBuffer(Int64 offset, Int64 count)
        {
            Int32 startBufferIndex = (Int32)(offset / _bufferSize) + 1;
            Int32 endBufferIndex = (Int32)((offset + count) / _bufferSize);

            for (Int32 bufferIndex = startBufferIndex; bufferIndex <= endBufferIndex; bufferIndex++)
                RemoveBuffer(bufferIndex);
        }

        /// <summary>
        /// Removes a buffer array.
        /// </summary>
        /// <param name="bufferIndex">The index of the buffer.</param>
        private void RemoveBuffer(Int32 bufferIndex)
        {
            if (!_isSingleAccess || !_buffers.ContainsKey(bufferIndex))
                return;
            
            if (_bitFlags[bufferIndex].All(value => value == 0))
            {
                _buffers.Remove(bufferIndex);
                _bitFlags.Remove(bufferIndex);
            }
        }

        /// <summary>
        /// Initializes the stream.
        /// </summary>
        /// <param name="type">The access type.</param>
        private void InitializeStream(StreamAccessType type)
        {
            if (_accessType != StreamAccessType.Undefined)
                return;

            switch (type)
            {
                case StreamAccessType.Readable:
                    _accessType = StreamAccessType.Readable;
                    _length = _baseStream.Length;
                    break;
                case StreamAccessType.Writable:
                    _accessType = StreamAccessType.Writable;
                    _length = 0;
                    break;
            }

            _position = 0;
            _buffers = new Dictionary<Int32, Byte[]>();

            if (_isSingleAccess)
                _bitFlags = new Dictionary<Int32, Byte[]>();
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">A value indicating whether disposing is performed on the object.</param>
        protected override void Dispose(Boolean disposing)
        {
            _disposed = true;

            if (disposing)
            {
                if (_accessType == StreamAccessType.Writable)
                    WriteDataIntoStream(true);

                if (_disposeBaseStream)
                    _baseStream.Dispose();
            }
        }

        #endregion
    }
}

