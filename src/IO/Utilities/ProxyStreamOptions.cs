// <copyright file="ProxyStreamOptions.cs" company="Eötvös Loránd University (ELTE)">
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

namespace AEGIS.IO.Utilities
{
    /// <summary>
    /// Defines options of the proxy stream.
    /// </summary>
    [Flags]
    public enum ProxyStreamOptions
    {
        /// <summary>
        /// Indicates that the default behavior is used.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Indicates to force proxy usage independently of underlying stream.
        /// </summary>
        ForceProxy = 1,

        /// <summary>
        /// Indicates that each byte can only be written read one times.
        /// </summary>
        SingleAccess = 2
    }
}
