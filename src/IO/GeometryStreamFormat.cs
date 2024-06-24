// <copyright file="GeometryStreamFormat.cs" company="Eötvös Loránd University (ELTE)">
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
using System.Linq;

namespace AEGIS.IO
{
    /// <summary>
    /// Represents a format of geometry stream.
    /// </summary>
    public class GeometryStreamFormat : IdentifiedObject
    {
        #region Public properties

        /// <summary>
        /// Gets or sets the version of the format.
        /// </summary>
        /// <value>The version of the format.</value>
        public String Version { get; private set; }

        /// <summary>
        /// Gets the file extensions of the format.
        /// </summary>
        /// <value>The array containing the file extensions of the format.</value>
        public String[] Extensions { get; private set; }

        /// <summary>
        /// Gets the internet media types of the format.
        /// </summary>
        /// <value>The array containing the internet media types of the format.</value>
        public String[] InternetMediaTypes { get; private set; }

        /// <summary>
        /// Gets the supported geometry types of the format.
        /// </summary>
        /// <value>The array containing the supported geometry types of the format.</value>
        public Type[] SupportedGeometries { get; private set; }

        /// <summary>
        /// Gets or sets the geometry models supported by the format.
        /// </summary>
        /// <value>The array containing the geometry models supported by the format.</value>
        public GeometryModel[] SupportedModels { get; private set; }

        /// <summary>
        /// Gets the parameters of the format.
        /// </summary>
        /// <value>The array containing the parameters of the format.</value>
        public GeometryStreamParameter[] Parameters { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryFileFormat" /> class.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="version">The version.</param>
        /// <param name="extensions">The extensions of the input/output data source.</param>
        /// <param name="internetMediaTypes">The internet media types.</param>
        /// <param name="supportedGeometries">The supported geometries of the format.</param>
        /// <param name="supportedModels">The supported models of the format.</param>
        /// <param name="parameters">The parameters of the format.</param>
        public GeometryStreamFormat(String identifier, String name, String version,
                                   String[] extensions, String[] internetMediaTypes,
                                   Type[] supportedGeometries, GeometryModel[] supportedModels,
                                   params GeometryStreamParameter[] parameters)
            : this(identifier, name, null, null, version, extensions, internetMediaTypes, supportedGeometries, supportedModels, parameters)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryFileFormat" /> class.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="aliases">The aliases.</param>
        /// <param name="version">The version.</param>
        /// <param name="extensions">The extensions of the input/output data source.</param>
        /// <param name="internetMediaTypes">The internet media types.</param>
        /// <param name="supportedGeometries">The supported geometries of the format.</param>
        /// <param name="supportedModels">The supported models of the format.</param>
        /// <param name="parameters">The parameters of the format.</param>
        public GeometryStreamFormat(String identifier, String name, String remarks, String[] aliases, String version,
                                    String[] extensions, String[] internetMediaTypes,
                                    Type[] supportedGeometries, GeometryModel[] supportedModels,
                                    params GeometryStreamParameter[] parameters)
            : base(identifier, name, remarks, aliases)
        {
            Version = version;
            Extensions = extensions;
            InternetMediaTypes = internetMediaTypes;
            SupportedGeometries = supportedGeometries;
            SupportedModels = supportedModels;
            Parameters = parameters;
        }

        #endregion

        /// <summary>
        /// Determines whether the specified geometry is supported by the format.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <returns><c>true</c> if the specified geometry is supported by the format; otherwise, <c>false</c>.</returns>
        public Boolean Supports(IGeometry geometry)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry", "The geometry is null.");

            return SupportedGeometries.Any(type => type.IsInstanceOfType(geometry));
        }
    }
}
