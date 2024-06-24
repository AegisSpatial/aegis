// <copyright file="IPolygon.cs" company="Eötvös Loránd University (ELTE)">
//     Copyright 2016-2019 Roberto Giachetta. Licensed under the
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

namespace AEGIS
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines behavior for polygon geometries.
    /// </summary>
    /// <remarks>
    /// A polygon is a planar surface defined by 1 exterior boundary (shell) and 0 or more interior boundaries (holes).
    /// </remarks>
    public interface IPolygon : ISurface, IBasicPolygon
    {
        /// <summary>
        /// Gets the shell of the polygon.
        /// </summary>
        /// <value>The linear ring representing the shell of the polygon.</value>
        new ILinearRing Shell { get; }

        /// <summary>
        /// Gets the holes of the polygon.
        /// </summary>
        /// <value>The read-only list containing the holes of the polygon.</value>
        new IReadOnlyList<ILinearRing> Holes { get; }

        /// <summary>
        /// Gets a hole at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the hole to get.</param>
        /// <returns>The hole at the specified index.</returns>
        /// <exception cref="System.InvalidOperationException">There are no holes in the polygon.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Index is less than 0.
        /// or
        /// The index is equal to or greater than the number of coordinates.
        /// </exception>
        new ILinearRing GetHole(Int32 index);

        /// <summary>
        /// Add a hole to the polygon.
        /// </summary>
        /// <param name="hole">The hole.</param>
        /// <exception cref="System.ArgumentNullException">The hole is null.</exception>
        /// <exception cref="System.ArgumentException">The reference system of the hole does not match the reference system of the polygon.</exception>
        void AddHole(ILinearRing hole);
    }
}
