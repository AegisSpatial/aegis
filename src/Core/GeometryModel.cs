// <copyright file="GeometryModel.cs" company="Eötvös Loránd University (ELTE)">
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

namespace AEGIS
{
    /// <summary>
    /// Defines the supported models for geometry representation.
    /// </summary>
    [Flags]
    public enum GeometryModel
    {
        /// <summary>
        /// No spatial or temporal support is specified.
        /// </summary>
        None = 1,

        /// <summary>
        /// Spatial with 2 dimensional coordinate system.
        /// </summary>
        Spatial2D = 2,

        /// <summary>
        /// Spatial with 3 dimensional coordinate system.
        /// </summary>
        Spatial3D = 4,

        /// <summary>
        /// Spatial with 2 or 3 dimensional coordinate system.
        /// </summary>
        Spatial = 6,

        /// <summary>
        /// Spatio-temporal with 2 dimensional spatial coordinate system and one dimensional temporal coordinate system.
        /// </summary>
        SpatioTemporal2D = 8,

        /// <summary>
        /// Spatio-temporal with 3 dimensional spatial coordinate system and one dimensional temporal coordinate system.
        /// </summary>
        SpatioTemporal3D = 16,

        /// <summary>
        /// Spatio-temporal with 2 or 3 dimensional spatial coordinate system and one dimensional temporal coordinate system.
        /// </summary>
        SpatioTemporal = 24,

        /// <summary>
        /// Spatial or spatio-temporal with 2 or 3 dimensional spatial coordinate system and optionally one dimensional temporal coordinate system.
        /// </summary>
        SpatialOrSpatioTemporal = 30,

        /// <summary>
        /// Support for all models.
        /// </summary>
        Any = 31
    }
}
