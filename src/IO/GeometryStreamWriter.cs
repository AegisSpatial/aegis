// <copyright file="GeometryStreamWriter.cs" company="Eötvös Loránd University (ELTE)">
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using AEGIS.IO.Utilities;
using AEGIS.Storage;
using AEGIS.Storage.FileSystems;

namespace AEGIS.IO
{
    /// <summary>
    /// Represents a base type for stream based geometry writing.
    /// </summary>
    public abstract class GeometryStreamWriter : IDisposable
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
        /// Exception message in case the geometry is null. This field is constant.
        /// </summary>
        protected const String MessageGeometryIsNull = "The geometry is null.";

        /// <summary>
        /// Exception message in case the type of the geometry is not supported. This field is constant.
        /// </summary>
        protected const String MessageGeometryIsNotSupported = "The type of the geometry is not supported by the format.";

        /// <summary>
        /// Exception message in case one or more of the geometries is not supported by the format. This field is constant.
        /// </summary>
        protected const String MessageGeometriesAreNotSupported = "One or more of the geometries is not supported by the format.";

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
        /// Exception message in case error occurred during stream writing. This field is constant.
        /// </summary>
        protected const String MessageContentWriteError = "Error occurred during stream writing.";

        #endregion

        #region Private fields

        /// <summary>
        /// Defines an empty collection of parameters. This field is read-only.
        /// </summary>
        private readonly static Dictionary<GeometryStreamParameter, Object> EmptyParameters = new Dictionary<GeometryStreamParameter, Object>();

        /// <summary>
        /// The parameters of the reader. This field is read-only.
        /// </summary>
        private readonly IDictionary<GeometryStreamParameter, Object> _parameters;

        /// <summary>
        /// The source stream.
        /// </summary>
        private Stream _sourceStream;

        /// <summary>
        /// The buffering mode.
        /// </summary>
        private BufferingMode _bufferingMode;

        /// <summary>
        /// A value indicating whether this instance is disposed.
        /// </summary>
        private Boolean _disposed;

        /// <summary>
        /// A value indicating whether to dispose the underlying stream.
        /// </summary>
        private Boolean _disposeSourceStream;

        #endregion

        #region Protected fields

        /// <summary>
        /// The underlying stream.
        /// </summary>
        protected readonly Stream _baseStream;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the format of the geometry stream.
        /// </summary>
        /// <value>The format of the geometry stream.</value>
        public GeometryStreamFormat Format { get; private set; }

        /// <summary>
        /// Gets the parameters of the reader.
        /// </summary>
        /// <value>The parameters of the reader stored as key/value pairs.</value>
        public IDictionary<GeometryStreamParameter, Object> Parameters { get { return new ReadOnlyDictionary<GeometryStreamParameter, Object>(_parameters != null ? _parameters : EmptyParameters); } }

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

        #endregion

        #region Constructors and destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryStreamWriter" /> class.
        /// </summary>
        /// <param name="path">The file path to be written.</param>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
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
        protected GeometryStreamWriter(String path, GeometryStreamFormat format, IDictionary<GeometryStreamParameter, Object> parameters)
            : this(ResolvePath(path), format, parameters)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryStreamWriter" /> class.
        /// </summary>
        /// <param name="path">The file path to be written.</param>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
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
        /// The parameters do not contain a required parameter value.
        /// or
        /// The type of a parameter value does not match the type specified by the format.
        /// </exception>
        /// <exception cref="System.IO.IOException">Exception occurred during stream opening.</exception>
        protected GeometryStreamWriter(Uri path, GeometryStreamFormat format, IDictionary<GeometryStreamParameter, Object> parameters)
            : this(ResolveStream(path), format, parameters)
        {
            Path = path;
            _disposeSourceStream = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryStreamWriter" /> class.
        /// </summary>
        /// <param name="stream">The file path to be written.</param>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The parameters.</param>
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
        protected GeometryStreamWriter(Stream stream, GeometryStreamFormat format, IDictionary<GeometryStreamParameter, Object> parameters)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", MessageStreamIsNull);
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

                _parameters = new Dictionary<GeometryStreamParameter, Object>(parameters);
            }

            Format = format;
            _bufferingMode = ResolveParameter<BufferingMode>(GeometryStreamParameters.BufferingMode);
            _disposeSourceStream = false;
            _disposed = false;
            _sourceStream = stream;

            // apply buffering
            switch (_bufferingMode)
            {
                case BufferingMode.Minimal:
                    _baseStream = new ProxyStream(_sourceStream, ProxyStreamOptions.ForceProxy | ProxyStreamOptions.SingleAccess);
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
        /// Finalizes an instance of the <see cref="GeometryStreamWriter" /> class.
        /// </summary>
        ~GeometryStreamWriter()
        {
            Dispose(false);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Writes the specified geometry.
        /// </summary>
        /// <param name="geometry">The geometry to be written.</param>
        /// <exception cref="System.ObjectDisposedException">Object is disposed.</exception>
        /// <exception cref="System.ArgumentNullException">The geometry is null.</exception>
        /// <exception cref="System.ArgumentException">The geometry is not supported by the format.</exception>
        /// <exception cref="System.IO.IOException">Exception occurred during stream writing.</exception>
        public void Write(IGeometry geometry)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (geometry == null)
                throw new ArgumentNullException("geometry", MessageGeometryIsNull);

            // required, because IGeometryCollection -> IGeometry conversion is prioritized over 
            //                   IGeometryCollection -> IEnumerable<IGeometry> conversion
            if (geometry is IEnumerable<IGeometry>)
            {
                Write(geometry as IEnumerable<IGeometry>);
                return;
            }

            if (!Format.Supports(geometry))
                throw new ArgumentException(MessageGeometryIsNotSupported, "geometry");

            try
            {
                ApplyWriteGeometry(geometry);
            }
            catch (Exception ex)
            {
                throw new IOException(MessageContentWriteError, ex);
            }
        }

        /// <summary>
        /// Writes the specified geometries.
        /// </summary>
        /// <param name="geometries">The geometries to be written.</param>
        /// <exception cref="System.ObjectDisposedException">Object is disposed.</exception>
        /// <exception cref="System.ArgumentNullException">The geometry is null.</exception>
        /// <exception cref="System.ArgumentException">One or more of the geometries is not supported by the format.</exception>
        /// <exception cref="System.IO.IOException">Exception occurred during stream writing.</exception>
        public void Write(IEnumerable<IGeometry> geometries)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (geometries == null)
                throw new ArgumentNullException("geometries", MessageGeometryIsNull);

            try
            {
                foreach (IGeometry geometry in geometries)
                {
                    if (geometry == null)
                        continue;

                    if (!Format.Supports(geometry))
                        throw new ArgumentException(MessageGeometriesAreNotSupported, "geometries");

                    ApplyWriteGeometry(geometry);
                }
            }
            catch (Exception ex)
            {
                throw new IOException(MessageContentWriteError, ex);
            }
        }

        /// <summary>
        /// Closes the writer and the underlying stream, and releases any system resources associated with the writer.
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
        /// Apply the write operation for the specified geometry.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        protected abstract void ApplyWriteGeometry(IGeometry geometry);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">A value indicating whether disposing is performed on the object.</param>
        protected virtual void Dispose(Boolean disposing)
        {
            _disposed = true;

            if (disposing)
            {
                switch (_bufferingMode)
                {
                    case BufferingMode.Minimal:
                    case BufferingMode.Maximal:
                        if (_baseStream != null)
                            _baseStream.Dispose();
                        break;
                }

                if (_disposeSourceStream && _sourceStream != null)
                    _sourceStream.Dispose();
            }
        }

        /// <summary>
        /// Resolves the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The specified parameter value or the default value if none specified.</returns>
        protected Object ResolveParameter(GeometryStreamParameter parameter)
        {
            if (_parameters != null && _parameters.ContainsKey(parameter))
                return _parameters[parameter];

            return parameter.DefaultValue;
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
                stream = fileSystem.OpenFile(path.AbsolutePath, FileMode.OpenOrCreate, FileAccess.Write);
            else
                stream = fileSystem.OpenFile(path.OriginalString, FileMode.OpenOrCreate, FileAccess.Write);

            // apply buffering
            switch (_bufferingMode)
            {
                case BufferingMode.Minimal:
                    return new ProxyStream(stream, ProxyStreamOptions.ForceProxy | ProxyStreamOptions.SingleAccess);
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
                    return fileSystem.OpenFile(path.AbsolutePath, FileMode.Create, FileAccess.Write);
                else
                    return fileSystem.OpenFile(path.OriginalString, FileMode.Create, FileAccess.Write);
            }
            catch (Exception ex)
            {
                throw new IOException(MessageContentOpenError, ex);
            }
        }

        #endregion
    }
}
