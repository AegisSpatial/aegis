// <copyright file="GeometryStreamReader.cs" company="Eötvös Loránd University (ELTE)">
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
using AEGIS.Collections;
using AEGIS.IO.Utilities;
using AEGIS.Storage;
using AEGIS.Storage.FileSystems;

namespace AEGIS.IO
{
    /// <summary>
    /// Represents a base type for stream based geometry reading.
    /// </summary>
    public abstract class GeometryStreamReader : IDisposable
    {
        #region Protected constant fields

        /// <summary>
        /// Exception message in case the path is null. This field is constant.
        /// </summary>
        protected const String MessagePathIsNull = "The path is null.";

        /// <summary>
        /// Exception message in case the path is empty, or consists only of whitespace characters. This field is constant.
        /// </summary>
        protected const String MessagePathIsEmpty = "The path is empty, or consists only of whitespace characters.";

        /// <summary>
        /// Exception message in case the path is in an invalid format. This field is constant.
        /// </summary>
        protected const String MessagePathIsInInvalidFormat = "The path is in an invalid format.";

        /// <summary>
        /// Exception message in case the stream is null. This field is constant.
        /// </summary>
        protected const String MessageStreamIsNull = "The stream is null.";

        /// <summary>
        /// Exception message in case the format is null. This field is constant.
        /// </summary>
        protected const String MessageFormatIsNull = "The format is null.";

        /// <summary>
        /// Exception message in case the format requires parameters which are not specified. This field is constant.
        /// </summary>
        protected const String MessageParametersNull = "The format requires parameters which are not specified.";

        /// <summary>
        /// Exception message in case the parameters do not contain a required parameter value. This field is constant.
        /// </summary>
        protected const String MessageParameterMissing = "The parameters do not contain a required parameter value ({0}).";

        /// <summary>
        /// Exception message in case the type of a parameter value does not match the type specified by the method. This field is constant.
        /// </summary>
        protected const String MessageParameterTypeError = "The type of a parameter value ({0}) does not match the type specified by the method.";

        /// <summary>
        /// Exception message in case the parameter value does not satisfy the conditions of the parameter. This field is constant.
        /// </summary>
        protected const String MessageParameterConditionError = "The parameter value ({0}) does not satisfy the conditions of the parameter.";

        /// <summary>
        /// Exception message in case error occurred during stream opening. This field is constant.
        /// </summary>
        protected const String MessageContentOpenError = "Error occurred during stream opening.";

        /// <summary>
        /// Exception message in case error occurred during stream reading. This field is constant.
        /// </summary>
        protected const String MessageContentReadError = "Error occurred during stream reading.";

        /// <summary>
        /// Exception message in case the stream content is in invalid. This field is constant.
        /// </summary>
        protected const String MessageContentInvalid = "The stream content is in invalid.";

        /// <summary>
        /// Exception message in case the stream header is in invalid. This field is constant.
        /// </summary>
        protected const String MessageHeaderInvalid = "The stream header is in invalid.";

        #endregion

        #region Private fields

        /// <summary>
        /// Defines an empty collection of parameters. This field is read-only.
        /// </summary>
        private static readonly Dictionary<GeometryStreamParameter, Object> EmptyParameters = new Dictionary<GeometryStreamParameter, Object>();

        /// <summary>
        /// The parameters of the reader. This field is read-only.
        /// </summary>
        private readonly IDictionary<GeometryStreamParameter, Object> _parameters;

        /// <summary>
        /// The source stream.
        /// </summary>
        private Stream _sourceStream;

        /// <summary>
        /// The factory used for producing geometries.
        /// </summary>
        private IGeometryFactory _factory;

        /// <summary>
        /// A value indicating whether this instance is disposed.
        /// </summary>
        private Boolean _disposed;

        /// <summary>
        /// A value indicating whether to dispose the underlying stream.
        /// </summary>
        private Boolean _disposeSourceStream;

        /// <summary>
        /// The buffering mode.
        /// </summary>
        private BufferingMode _bufferingMode;

        #endregion

        #region Protected fields

        /// <summary>
        /// The underlying stream.
        /// </summary>
        protected readonly Stream _baseStream;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the factory used for geometry production.
        /// </summary>
        /// <value>The factory used by the reader for geometry production.</value>
        public IGeometryFactory Factory { get { return _factory; } }

        /// <summary>
        /// Gets the format of the geometry stream.
        /// </summary>
        /// <value>The format of the geometry stream.</value>
        public GeometryStreamFormat Format { get; private set; }

        /// <summary>
        /// Gets the parameters of the reader.
        /// </summary>
        /// <value>The parameters of the reader stored as key/value pairs.</value>
        public IReadOnlyDictionary<GeometryStreamParameter, Object> Parameters { get { return (_parameters != null ? _parameters : EmptyParameters).AsReadOnly(); } }

        /// <summary>
        /// Gets the path of the data.
        /// </summary>
        /// <value>The full path of the data.</value>
        public Uri Path { get; private set; }

        /// <summary>
        /// Gets the underlying stream.
        /// </summary>
        /// <value>The underlying stream.</value>
        public Stream BaseStream { get { return _baseStream; } }

        /// <summary>
        /// Gets a value that indicates whether the current stream position is at the end of the stream.
        /// </summary>
        /// <value><c>true</c> if the current stream position is at the end of the stream; otherwise, <c>false</c>.</value>
        /// <exception cref="System.ObjectDisposedException">Object is disposed.</exception>
        public virtual Boolean EndOfStream
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);
                return GetEndOfStream();
            }
        }

        #endregion

        #region Constructor and destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryStreamReader" /> class.
        /// </summary>
        /// <param name="path">The file path to be read.</param>
        /// <param name="format">The format of the stream reader.</param>
        /// <param name="parameters">The parameters of the reader for the specific stream.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The path is null.
        /// or
        /// The format is null.
        /// or
        /// The format requires parameters which are not specified.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// The path is empty, or consists only of whitespace characters.
        /// or
        /// The path is in an invalid format.
        /// or
        /// The parameters do not contain a required parameter value.
        /// or
        /// The type of a parameter value does not match the type specified by the format.
        /// or
        /// The parameter value does not satisfy the conditions of the parameter.
        /// </exception>
        /// <exception cref="System.IO.IOException">Exception occurred during stream opening.</exception>
        protected GeometryStreamReader(String path, GeometryStreamFormat format, IDictionary<GeometryStreamParameter, Object> parameters)
            : this(ResolvePath(path), format, parameters)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryStreamReader" /> class.
        /// </summary>
        /// <param name="path">The file path to be read.</param>
        /// <param name="format">The format of the stream reader.</param>
        /// <param name="parameters">The parameters of the reader for the specific stream.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The path is null.
        /// or
        /// The format is null.
        /// or
        /// The format requires parameters which are not specified.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// The parameters do not contain a required parameter value.
        /// or
        /// The type of a parameter value does not match the type specified by the format.
        /// </exception>
        /// <exception cref="System.IO.IOException">Exception occurred during stream opening.</exception>
        protected GeometryStreamReader(Uri path, GeometryStreamFormat format, IDictionary<GeometryStreamParameter, Object> parameters)
            : this(ResolveStream(path), format, parameters)
        {
            Path = path;
            _disposeSourceStream = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryStreamReader" /> class.
        /// </summary>
        /// <param name="stream">The stream to be read.</param>
        /// <param name="format">The format of the stream reader.</param>
        /// <param name="parameters">The parameters of the reader for the specific stream.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The stream is null.
        /// or
        /// The format is null.
        /// or
        /// The format requires parameters which are not specified.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// The parameters do not contain a required parameter value.
        /// or
        /// The type of a parameter value does not match the type specified by the format.
        /// </exception>
        protected GeometryStreamReader(Stream stream, GeometryStreamFormat format, IDictionary<GeometryStreamParameter, Object> parameters)
            : this(format, parameters)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", MessageStreamIsNull);

            // store parameters
            _disposeSourceStream = false;
            _sourceStream = stream;

            // apply buffering
            switch (_bufferingMode)
            {
                case BufferingMode.Minimal:
                    _baseStream = new ProxyStream(stream, ProxyStreamOptions.ForceProxy | ProxyStreamOptions.SingleAccess);
                    break;
                case BufferingMode.Maximal:
                    _baseStream = new MemoryBufferedStream(_sourceStream);
                    break;
                default:
                    _baseStream = _sourceStream;
                    break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryStreamReader" /> class.
        /// </summary>
        /// <param name="format">The format of the stream reader.</param>
        /// <param name="parameters">The parameters of the reader.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The format is null.
        /// or
        /// The format requires parameters which are not specified.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// The parameters do not contain a required parameter value.
        /// or
        /// The type of a parameter value does not match the type specified by the format.
        /// </exception>
        protected GeometryStreamReader(GeometryStreamFormat format, IDictionary<GeometryStreamParameter, Object> parameters)
        {
            if (format == null)
                throw new ArgumentNullException("format", MessageFormatIsNull);
            if (parameters == null && format.Parameters != null && format.Parameters.Length > 0)
                throw new ArgumentNullException("parameters", MessageParametersNull);

            if (parameters != null && format.Parameters != null)
            {
                foreach (GeometryStreamParameter parameter in format.Parameters)
                {
                    // check parameter existence
                    if (!parameter.IsOptional && (!parameters.ContainsKey(parameter) || parameters[parameter] == null))
                        throw new ArgumentException(String.Format(MessageParameterMissing, parameter.Name), "parameters");

                    if (parameters.ContainsKey(parameter))
                    {
                        // check parameter type
                        if (!(parameter.Type.GetInterfaces().Contains(typeof(IConvertible)) && parameters[parameter] is IConvertible) &&
                            !parameter.Type.Equals(parameters[parameter].GetType()) &&
                            !parameters[parameter].GetType().IsSubclassOf(parameter.Type) &&
                            !parameters[parameter].GetType().GetInterfaces().Contains(parameter.Type))
                            throw new ArgumentException(String.Format(MessageParameterTypeError, parameter.Name), "parameters");

                        // check parameter value
                        if (!parameter.IsValid(parameters[parameter]))
                            throw new ArgumentException(String.Format(MessageParameterConditionError, parameter.Name), "parameters");
                    }
                }
            }

            // store parameters
            Format = format;
            _parameters = parameters;
            _disposeSourceStream = true;
            _disposed = false;
            _bufferingMode = ResolveParameter<BufferingMode>(GeometryStreamParameters.BufferingMode);

            // resolve factory or factory type
            if (IsProvidedParameter(GeometryStreamParameters.GeometryFactory))
                _factory = ResolveParameter<IGeometryFactory>(GeometryStreamParameters.GeometryFactory);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="GeometryStreamReader" /> class.
        /// </summary>
        ~GeometryStreamReader()
        {
            Dispose(false);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Reads a single geometry from the data stream.
        /// </summary>
        /// <returns>The geometry read from the data stream.</returns>
        /// <exception cref="System.ObjectDisposedException">Object is disposed.</exception>
        /// <exception cref="System.IO.InvalidDataException">Exception occurred during stream reading.</exception>
        public IGeometry Read()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            try
            {
                return ApplyReadGeometry();
            }
            catch (Exception ex)
            {
                throw new InvalidDataException(MessageContentReadError, ex);
            }
        }

        /// <summary>
        /// Reads a number of geometries from the data stream.
        /// </summary>
        /// <param name="count">The number of geometries to read.</param>
        /// <returns>A list containing the geometries read from the data stream.</returns>
        /// <exception cref="System.ObjectDisposedException">Object is disposed.</exception>
        /// <exception cref="System.IO.IOException">Exception occurred during stream reading.</exception>
        public IList<IGeometry> Read(Int32 count)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            try
            {
                List<IGeometry> result = new List<IGeometry>(count);
                for (Int32 i = 0; i < count && !EndOfStream; i++)
                {
                    result.Add(ApplyReadGeometry());
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new IOException(MessageContentReadError, ex);
            }
        }

        /// <summary>
        /// Reads all geometries from the data stream.
        /// </summary>
        /// <returns>A list containing the geometries read from the data stream.</returns>
        /// <exception cref="System.ObjectDisposedException">Object is disposed.</exception>
        /// <exception cref="System.IO.IOException">Exception occurred during stream reading.</exception>
        public IList<IGeometry> ReadToEnd()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            try
            {
                List<IGeometry> result = new List<IGeometry>();
                while (!EndOfStream)
                {
                    result.Add(ApplyReadGeometry());
                }
                return result.ToArray();
            }
            catch (Exception ex)
            {
                throw new IOException("Exception occurred during stream reading.", ex);
            }
        }

        /// <summary>
        /// Closes the reader and the underlying stream, and releases any system resources associated with the reader.
        /// </summary>
        public virtual void Close()
        {
            Dispose();
        }

        #endregion

        #region IDisposable methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Returns a value indicating whether the end of the stream is reached.
        /// </summary>
        /// <returns><c>true</c> if the end of the stream is reached; otherwise, <c>false</c>.</returns>
        protected abstract Boolean GetEndOfStream();

        /// <summary>
        /// Apply the read operation for a geometry.
        /// </summary>
        /// <returns>The geometry read from the stream.</returns>
        protected abstract IGeometry ApplyReadGeometry();

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">A value indicating whether disposing is performed on the object.</param>
        protected virtual void Dispose(Boolean disposing)
        {            
            _disposed = true;

            if (disposing)
            {
                if (_baseStream != null)
                {
                    switch (_bufferingMode)
                    {
                        case BufferingMode.Minimal:
                        case BufferingMode.Maximal:
                            _baseStream.Dispose();
                            break;
                    }

                    if (_disposeSourceStream)
                        _sourceStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Resolves the specified parameter.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The specified parameter value or the default value if none specified.</returns>
        protected T ResolveParameter<T>(GeometryStreamParameter parameter)
        {
            if (_parameters != null && _parameters.ContainsKey(parameter) && _parameters[parameter] is T)
                return (T)_parameters[parameter];

            return (T)parameter.DefaultValue;
        }

        /// <summary>
        /// Determines whether the specified parameter is provided.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns><c>true</c> if the parameter is provided; otherwise, <c>false</c>.</returns>
        protected Boolean IsProvidedParameter(GeometryStreamParameter parameter)
        {
            return _parameters != null && _parameters.ContainsKey(parameter);
        }

        /// <summary>
        /// Opens the file on the specified path.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The stream of the file.</returns>
        protected Stream OpenPath(String path)
        {
            return OpenPath(ResolvePath(path));
        }

        /// <summary>
        /// Opens the file on the specified path.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The stream of the file.</returns>
        protected Stream OpenPath(Uri path)
        {
            IFileSystem fileSystem = FileSystemContainer.GetFileSystemForScheme(path.Scheme);
            Stream stream;

            if (path.IsAbsoluteUri)
                stream = fileSystem.OpenFile(path.AbsolutePath, FileMode.Open, FileAccess.Read);
            else
                stream = fileSystem.OpenFile(path.OriginalString, FileMode.Open, FileAccess.Read);

            // apply buffering
            switch (_bufferingMode)
            {
                case BufferingMode.Minimal:
                    return new ProxyStream(stream, true, ProxyStreamOptions.ForceProxy | ProxyStreamOptions.SingleAccess);
                case BufferingMode.Maximal:
                    return new MemoryBufferedStream(stream, true);
                default:
                    return stream;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Resolves the path.
        /// </summary>
        /// <param name="path">The path string.</param>
        /// <returns>The path URI.</returns>
        /// <exception cref="System.ArgumentNullException">The path is null.</exception>
        /// <exception cref="System.ArgumentException">
        /// The path is empty, or consists only of whitespace characters.
        /// or
        /// The path is in an invalid format.
        /// </exception>
        private static Uri ResolvePath(String path)
        {
            Uri pathUri;

            if (path == null)
                throw new ArgumentNullException("path", MessagePathIsNull);
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException(MessagePathIsEmpty, "path");
            if (!Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out pathUri))
                throw new ArgumentException(MessagePathIsInInvalidFormat, "path");

            return pathUri;
        }

        /// <summary>
        /// Resolves the stream.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The stream.</returns>
        /// <exception cref="System.IO.IOException">Exception occurred during stream opening.</exception>
        private static Stream ResolveStream(Uri path)
        {
            try
            {
                IFileSystem fileSystem = FileSystemContainer.GetFileSystemForScheme(path.Scheme);
                if (path.IsAbsoluteUri)
                    return fileSystem.OpenFile(path.AbsolutePath, FileMode.Open, FileAccess.Read);
                else
                    return fileSystem.OpenFile(path.OriginalString, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex)
            {
                throw new IOException(MessageContentOpenError, ex);
            }
        }

        #endregion
    }
}
