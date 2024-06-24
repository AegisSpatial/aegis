// <copyright file="GeometryStreamParameters.cs" company="Eötvös Loránd University (ELTE)">
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
using System.Linq;

namespace AEGIS.IO
{
    /// <summary>
    /// Represents a collection of known <see cref="GeometryStreamParameter" /> instances.
    /// </summary>
    [IdentifiedObjectCollection(typeof(GeometryStreamParameter))]
    public class GeometryStreamParameters
    {
        #region Query fields

        private static GeometryStreamParameter[] _all;

        #endregion

        #region Query properties

        /// <summary>
        /// Gets all <see cref="GeometryStreamParameter" /> instances within the collection.
        /// </summary>
        /// <value>A read-only list containing all <see cref="GeometryStreamParameter" /> instances within the collection.</value>
        public static IList<GeometryStreamParameter> All
        {
            get
            {
                if (_all == null)
                    _all = typeof(GeometryStreamParameters).GetProperties().
                                                            Where(property => property.Name != "All").
                                                            Select(property => property.GetValue(null, null) as GeometryStreamParameter).
                                                            ToArray();
                return Array.AsReadOnly(_all);
            }
        }

        #endregion

        #region Query methods

        /// <summary>
        /// Returns all <see cref="GeometryStreamParameter" /> instances matching a specified identifier.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <returns>A list containing the <see cref="GeometryStreamParameter" /> instances that match the specified identifier.</returns>
        public static IList<GeometryStreamParameter> FromIdentifier(String identifier)
        {
            if (identifier == null)
                return null;

            return All.Where(obj => System.Text.RegularExpressions.Regex.IsMatch(obj.Identifier, identifier)).ToList().AsReadOnly();
        }

        /// <summary>
        /// Returns all <see cref="GeometryStreamParameter" /> instances matching a specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A list containing the <see cref="GeometryStreamParameter" /> instances that match the specified name.</returns>
        public static IList<GeometryStreamParameter> FromName(String name)
        {
            if (name == null)
                return null;

            return All.Where(obj => System.Text.RegularExpressions.Regex.IsMatch(obj.Name, name) ||
                                    obj.Aliases != null && obj.Aliases.Any(alias => System.Text.RegularExpressions.Regex.IsMatch(alias, name, System.Text.RegularExpressions.RegexOptions.IgnoreCase))).ToList().AsReadOnly();
        }

        #endregion

        #region Private static fields

        private static GeometryStreamParameter _geometryFactory;
        private static GeometryStreamParameter _geometryFactoryType;
        private static GeometryStreamParameter _bufferingMode;

        #endregion

        #region Public static fields

        /// <summary>
        /// Geometry factory.
        /// </summary>
        public static GeometryStreamParameter GeometryFactory
        {
            get
            {
                return _geometryFactory ?? (_geometryFactory =
                    GeometryStreamParameter.CreateOptionalParameter<IGeometryFactory>("AEGIS::620002", "Geometry factory",
                                                                                      "The geometry factory used to produce the instances read from the specified format. If geometry factory is specified, the reference system of the factory is used instead of the reference system provided by the source."));
            }
        }

        /// <summary>
        /// Geometry factory type.
        /// </summary>
        public static GeometryStreamParameter GeometryFactoryType
        {
            get
            {
                return _geometryFactoryType ?? (_geometryFactoryType =
                    GeometryStreamParameter.CreateOptionalParameter<Type>("AEGIS::620001", "Geometry factory type",
                                                                          "The type of the geometry factory used to produce the instances read from the specified format. If geometry factory type is specified, an instance of this type will be used with the reference system provided by the source.",
                                                                          Conditions.Implements<IGeometryFactory>()));
            }
        }

        /// <summary>
        /// Buffering mode.
        /// </summary>
        public static GeometryStreamParameter BufferingMode
        {
            get
            {
                return _bufferingMode ?? (_bufferingMode =
                    GeometryStreamParameter.CreateOptionalParameter<BufferingMode>("AEGIS::620008", "buffering mode",
                                                                                   "The buffering mode.",
                                                                                   IO.BufferingMode.None));
            }
        }

        #endregion
    }
}
