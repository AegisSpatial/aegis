// <copyright file="FileSystemContainer.cs" company="Eötvös Loránd University (ELTE)">
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

namespace AEGIS.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a static factory class for <see cref="IFileSystem"/> subtypes.
    /// </summary>
    /// <author>Máté Cserép</author>
    public static class FileSystemContainer
    {
        /// <summary>
        /// Registry of scheme and the corresponding <see cref="IFileSystem"/> subtypes.
        /// </summary>
        private static readonly Dictionary<string, Type> Registry = new Dictionary<string, Type>();

        /// <summary>
        /// Initializes static members of the <see cref="FileSystemContainer"/> class.
        /// </summary>
        static FileSystemContainer()
        {
            RegisterAllSchemes();
        }

        /// <summary>
        /// Gets a value indicating whether registry is initialized.
        /// </summary>
        /// <remarks>
        /// The registry is considered initialized if it is not empty.
        /// </remarks>
        public static bool IsInitialized => Registry.Count > 0;

        /// <summary>
        /// Registers a new file system scheme.
        /// </summary>
        /// <param name="scheme">The scheme of the file system.</param>
        /// <param name="fileSystemType">Type of the file system.</param>
        /// <exception cref="ArgumentNullException">The scheme cannot be null.</exception>
        /// <exception cref="ArgumentNullException">The file system type cannot be null.</exception>
        /// <exception cref="ArgumentException">Only subtypes of the IFileSystem interface can be registered.</exception>
        public static void RegisterScheme(string scheme, Type fileSystemType)
        {
            if(scheme == null)
                throw new ArgumentNullException(nameof(scheme), "The scheme is null.");
            if(fileSystemType == null)
                throw new ArgumentNullException(nameof(fileSystemType), "The file system type is null.");
            if (!fileSystemType.IsSubclassOf(typeof(IFileSystem)))
                throw new ArgumentException("Only subtypes of the IFileSystem interface can be registered.", nameof(fileSystemType));

            Registry.Add(scheme, fileSystemType);
        }

        /// <summary>
        /// Unregisters a file system scheme.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <exception cref="ArgumentNullException">The scheme cannot be null.</exception>
        public static void UnregisterScheme(string scheme)
        {
            if (scheme == null)
                throw new ArgumentNullException(nameof(scheme), "The scheme is null.");

            Registry.Remove(scheme);
        }

        /// <summary>
        /// Instantiates an <see cref="IFileSystem"/> object for the requested scheme.
        /// </summary>
        /// <param name="scheme">The scheme.</param>
        /// <returns>A new file system object for the given scheme.</returns>
        /// <exception cref="ArgumentNullException">The scheme cannot be null.</exception>
        public static IFileSystem GetFileSystemForScheme(string scheme)
        {
            if (scheme == null)
                throw new ArgumentNullException(nameof(scheme), "The scheme is null.");

            return (IFileSystem)Activator.CreateInstance(Registry[scheme]);

        }

        /// <summary>
        /// Instantiates an appropriate <see cref="IFileSystem"/> object for the requested path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A new file system object for the given path.</returns>
        /// <exception cref="ArgumentNullException">The path is null.</exception>
        public static IFileSystem GetFileSystemForPath(Uri path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path), "The path is null.");

            return GetFileSystemForScheme(path.Scheme);
        }

        /// <summary>
        /// Instantiates an appropriate <see cref="IFileSystem"/> object for the requested path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A new file system object for the given path.</returns>
        /// <exception cref="ArgumentNullException">The path is null.</exception>
        public static IFileSystem GetFileSystemForPath(string path)
        {
            Uri pathUri;

            if (path == null)
                throw new ArgumentNullException(nameof(path), "The path is null.");
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("The path is empty.", nameof(path));
            if (!Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out pathUri))
                throw new ArgumentException("The path is in an invalid format.", nameof(path));

            return GetFileSystemForPath(pathUri);
        }

        /// <summary>
        /// Registers all schemes available in the loaded assemblies.
        /// </summary>
        public static void RegisterAllSchemes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(IFileSystem)))
                    .Where(t => !t.IsAbstract);

                foreach (var type in types)
                {
                    var schemeField = type.GetField("UriScheme");
                    RegisterScheme(schemeField.GetRawConstantValue().ToString(), type);
                }
            }
        }
    }
}
