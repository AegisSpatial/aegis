// <copyright file="MemoryBufferedStream.cs" company="Eötvös Loránd University (ELTE)">
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
using System.IO;

namespace AEGIS.IO.Utilities
{
    /// <summary>
    /// Represents a memory buffered stream.
    /// </summary>
    public class MemoryBufferedStream : Stream
    {
        #region Private fields

        /// <summary>
        /// The source stream.
        /// </summary>
        private Stream _sourceStream;

        /// <summary>
        /// The memory stream.
        /// </summary>
        private MemoryStream _memoryStream;

        /// <summary>
        /// A value indicating whether to dispose the source stream.
        /// </summary>
        private Boolean _disposeSourceStream;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryBufferedStream" /> class.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <param name="disposeSourceStream">A value indicating whether to dispose the source stream.</param>
        public MemoryBufferedStream(Stream sourceStream, Boolean disposeSourceStream = false)
        {
            _sourceStream = sourceStream;
            _memoryStream = new MemoryStream();
            _disposeSourceStream = disposeSourceStream;

            if (_sourceStream.CanRead)
            {
                Byte[] inputBytes = new Byte[1 << 20];
                Int32 bytesRead = 0;

                while ((bytesRead = _sourceStream.Read(inputBytes, 0, inputBytes.Length)) > 0)
                {
                    _memoryStream.Write(inputBytes, 0, bytesRead);
                }
                _memoryStream.Flush();
                _memoryStream.Seek(0, SeekOrigin.Begin);
            }
        }

        #endregion

        #region Stream properties

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override Boolean CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        public override Boolean CanSeek
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
        public override Boolean CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        public override Int64 Length
        {
            get { return _memoryStream.Length; }
        }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        /// <returns>The current position within the stream.</returns>
        public override Int64 Position
        {
            get
            {
                return _memoryStream.Position;
            }
            set
            {
                _memoryStream.Position = value;
            }
        }

        #endregion

        #region Stream methods

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            _memoryStream.Flush();
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
        {
            return _memoryStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        public override Int64 Seek(Int64 offset, SeekOrigin origin)
        {
            return _memoryStream.Seek(offset, origin);
        }

        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        public override void SetLength(Int64 value)
        {
            _memoryStream.SetLength(value);
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(Byte[] buffer, Int32 offset, Int32 count)
        {
            _memoryStream.Write(buffer, offset, count);
        }

        #endregion

        #region Protected Stream methods

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="Stream" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                if (_sourceStream.CanWrite)
                {
                    _memoryStream.Seek(0, SeekOrigin.Begin);
                    Byte[] inputBytes = new Byte[1 << 20];
                    Int32 bytesRead = 0;

                    while ((bytesRead = _memoryStream.Read(inputBytes, 0, inputBytes.Length)) > 0)
                    {
                        _sourceStream.Write(inputBytes, 0, bytesRead);
                    }
                    _sourceStream.Flush();
                }

                _memoryStream.Dispose();

                if (_disposeSourceStream)
                    _sourceStream.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
