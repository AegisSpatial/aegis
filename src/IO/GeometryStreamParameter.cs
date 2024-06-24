// <copyright file="GeometryStreamParameter.cs" company="Eötvös Loránd University (ELTE)">
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

namespace AEGIS.IO
{
    /// <summary>
    /// Represents a parameter of a geometry stream format.
    /// </summary>
    public class GeometryStreamParameter : IdentifiedObject
    {
        #region Private fields

        private readonly Type _type;
        private readonly Boolean _isOptional;
        private readonly Object _defaultValue;
        private readonly Predicate<Object>[] _conditions;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the type declaration of the parameter.
        /// </summary>
        /// <value>The type declaration of the parameter.</value>
        public Type Type { get { return _type; } }

        /// <summary>
        /// Gets a value indication whether the parameter is optional.
        /// </summary>
        /// <value><c>true</c> if the parameter is optional; otherwise, <c>false</c>.</value>
        public Boolean IsOptional { get { return _isOptional; } }

        /// <summary>
        /// Gets the default value of the parameter.
        /// </summary>
        /// <value>The default value of the parameter.</value>
        public Object DefaultValue { get { return _defaultValue; } }

        /// <summary>
        /// Gets the conditions the parameter value must satisfy.
        /// </summary>
        /// <value>The conditions the parameter value must satisfy.</value>
        public IList<Predicate<Object>> Conditions { get { return Array.AsReadOnly(_conditions); } }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryStreamParameter" /> class.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="aliases">The aliases.</param>
        /// <param name="type">The type declaration of the parameter.</param>
        /// <param name="isOptional">A value indicating whether the parameter is optional.</param>
        /// <param name="defaultValue">The default value of the parameter (if optional).</param>
        /// <param name="conditions">The conditions the parameter value must satisfy.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public GeometryStreamParameter(String identifier, String name, String remarks, String[] aliases, Type type, Boolean isOptional, Object defaultValue, params Predicate<Object>[] conditions)
            : base(identifier, name, remarks, aliases)
        {
            if (type == null)
                throw new ArgumentNullException("type", "The type is null.");

            _type = type;
            _isOptional = isOptional;
            _defaultValue = defaultValue ?? (type.IsValueType ? Activator.CreateInstance(type) : null);
            _conditions = conditions;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Determines whether the specified value is valid for the parameter.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if <paramref name="value" /> meets all conditions; otherwise, <c>false</c>.</returns>
        public Boolean IsValid(Object value)
        {
            if (_conditions == null || _conditions.Length == 0)
                return true;

            for (Int32 i = 0; i < _conditions.Length; i++)
                if (!_conditions[i](value)) return false;

            return true;
        }

        #endregion

        #region Public static factory methods

        /// <summary>
        /// Creates an optional <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateOptionalParameter<T>(String identifier, String name, String remarks)
        {
            return new GeometryStreamParameter(identifier, name, remarks, null, typeof(T), true, null, null);
        }

        /// <summary>
        /// Creates an optional <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="defaultValue">The default value of the parameter.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateOptionalParameter<T>(String identifier, String name, String remarks, T defaultValue)
        {
            return new GeometryStreamParameter(identifier, name, remarks, null, typeof(T), true, defaultValue, null);
        }

        /// <summary>
        /// Creates an optional <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="aliases">The aliases.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateOptionalParameter<T>(String identifier, String name, String remarks, String[] aliases)
        {
            return new GeometryStreamParameter(identifier, name, remarks, aliases, typeof(T), true, null, null);
        }

        /// <summary>
        /// Creates an optional <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="aliases">The aliases.</param>
        /// <param name="defaultValue">The default value of the parameter.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateOptionalParameter<T>(String identifier, String name, String remarks, String[] aliases, T defaultValue)
        {
            return new GeometryStreamParameter(identifier, name, remarks, aliases, typeof(T), true, defaultValue, null);
        }

        /// <summary>
        /// Creates an optional <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="conditions">The conditions the parameter value must satisfy.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateOptionalParameter<T>(String identifier, String name, String remarks, params Predicate<Object>[] conditions)
        {
            return new GeometryStreamParameter(identifier, name, remarks, null, typeof(T), true, null, conditions);
        }

        /// <summary>
        /// Creates an optional <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="defaultValue">The default value of the parameter.</param>
        /// <param name="conditions">The conditions the parameter value must satisfy.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateOptionalParameter<T>(String identifier, String name, String remarks, T defaultValue, params Predicate<Object>[] conditions)
        {
            return new GeometryStreamParameter(identifier, name, remarks, null, typeof(T), true, defaultValue, conditions);
        }

        /// <summary>
        /// Creates an optional <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="aliases">The aliases.</param>
        /// <param name="conditions">The conditions the parameter value must satisfy.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateOptionalParameter<T>(String identifier, String name, String remarks, String[] aliases, params Predicate<Object>[] conditions)
        {
            return new GeometryStreamParameter(identifier, name, remarks, aliases, typeof(T), true, null, conditions);
        }

        /// <summary>
        /// Creates an optional <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="aliases">The aliases.</param>
        /// <param name="defaultValue">The default value of the parameter.</param>
        /// <param name="conditions">The conditions the parameter value must satisfy.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateOptionalParameter<T>(String identifier, String name, String remarks, String[] aliases, T defaultValue, params Predicate<Object>[] conditions)
        {
            return new GeometryStreamParameter(identifier, name, remarks, aliases, typeof(T), true, defaultValue, conditions);
        }


        /// <summary>
        /// Creates an optional <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type declaration of the parameter.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateOptionalParameter(String identifier, String name, String remarks, Type type)
        {
            return new GeometryStreamParameter(identifier, name, remarks, null, type, true, null, null);
        }

        /// <summary>
        /// Creates an optional <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type declaration of the parameter.</param>
        /// <param name="defaultValue">The default value of the parameter.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateOptionalParameter(String identifier, String name, String remarks, Type type, Object defaultValue)
        {
            return new GeometryStreamParameter(identifier, name, remarks, null, type, true, defaultValue, null);
        }

        /// <summary>
        /// Creates an optional <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="aliases">The aliases.</param>
        /// <param name="type">The type declaration of the parameter.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateOptionalParameter(String identifier, String name, String remarks, String[] aliases, Type type)
        {
            return new GeometryStreamParameter(identifier, name, remarks, aliases, type, true, null, null);
        }

        /// <summary>
        /// Creates an optional <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="aliases">The aliases.</param>
        /// <param name="type">The type declaration of the parameter.</param>
        /// <param name="defaultValue">The default value of the parameter.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateOptionalParameter(String identifier, String name, String remarks, String[] aliases, Type type, Object defaultValue)
        {
            return new GeometryStreamParameter(identifier, name, remarks, aliases, type, true, defaultValue, null);
        }

        /// <summary>
        /// Creates an optional <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type declaration of the parameter.</param>
        /// <param name="conditions">The conditions the parameter value must satisfy.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateOptionalParameter(String identifier, String name, String remarks, Type type, params Predicate<Object>[] conditions)
        {
            return new GeometryStreamParameter(identifier, name, remarks, null, type, true, null, conditions);
        }

        /// <summary>
        /// Creates an optional <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type declaration of the parameter.</param>
        /// <param name="defaultValue">The default value of the parameter.</param>
        /// <param name="conditions">The conditions the parameter value must satisfy.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateOptionalParameter(String identifier, String name, String remarks, Type type, Object defaultValue, params Predicate<Object>[] conditions)
        {
            return new GeometryStreamParameter(identifier, name, remarks, null, type, true, defaultValue, conditions);
        }

        /// <summary>
        /// Creates an optional <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="aliases">The aliases.</param>
        /// <param name="type">The type declaration of the parameter.</param>
        /// <param name="conditions">The conditions the parameter value must satisfy.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateOptionalParameter(String identifier, String name, String remarks, String[] aliases, Type type, params Predicate<Object>[] conditions)
        {
            return new GeometryStreamParameter(identifier, name, remarks, aliases, type, true, null, conditions);
        }

        /// <summary>
        /// Creates an optional <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="aliases">The aliases.</param>
        /// <param name="type">The type declaration of the parameter.</param>
        /// <param name="defaultValue">The default value of the parameter.</param>
        /// <param name="conditions">The conditions the parameter value must satisfy.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateOptionalParameter(String identifier, String name, String remarks, String[] aliases, Type type, Object defaultValue, params Predicate<Object>[] conditions)
        {
            return new GeometryStreamParameter(identifier, name, remarks, aliases, type, true, defaultValue, conditions);
        }

        /// <summary>
        /// Creates a required <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateRequiredParameter<T>(String identifier, String name, String remarks)
        {
            return new GeometryStreamParameter(identifier, name, remarks, null, typeof(T), false, null, null);
        }

        /// <summary>
        /// Creates a required <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="defaultValue">The default value of the parameter.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateRequiredParameter<T>(String identifier, String name, String remarks, T defaultValue)
        {
            return new GeometryStreamParameter(identifier, name, remarks, null, typeof(T), false, defaultValue, null);
        }

        /// <summary>
        /// Creates a required <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="aliases">The aliases.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateRequiredParameter<T>(String identifier, String name, String remarks, String[] aliases)
        {
            return new GeometryStreamParameter(identifier, name, remarks, aliases, typeof(T), false, null, null);
        }

        /// <summary>
        /// Creates a required <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="aliases">The aliases.</param>
        /// <param name="defaultValue">The default value of the parameter.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateRequiredParameter<T>(String identifier, String name, String remarks, String[] aliases, T defaultValue)
        {
            return new GeometryStreamParameter(identifier, name, remarks, aliases, typeof(T), false, defaultValue, null);
        }

        /// <summary>
        /// Creates a required <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="conditions">The conditions the parameter value must satisfy.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateRequiredParameter<T>(String identifier, String name, String remarks, params Predicate<Object>[] conditions)
        {
            return new GeometryStreamParameter(identifier, name, remarks, null, typeof(T), false, null, conditions);
        }

        /// <summary>
        /// Creates a required <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="defaultValue">The default value of the parameter.</param>
        /// <param name="conditions">The conditions the parameter value must satisfy.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateRequiredParameter<T>(String identifier, String name, String remarks, T defaultValue, params Predicate<Object>[] conditions)
        {
            return new GeometryStreamParameter(identifier, name, remarks, null, typeof(T), false, defaultValue, conditions);
        }

        /// <summary>
        /// Creates a required <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="aliases">The aliases.</param>
        /// <param name="conditions">The conditions the parameter value must satisfy.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateRequiredParameter<T>(String identifier, String name, String remarks, String[] aliases, params Predicate<Object>[] conditions)
        {
            return new GeometryStreamParameter(identifier, name, remarks, aliases, typeof(T), false, null, conditions);
        }

        /// <summary>
        /// Creates a required <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="aliases">The aliases.</param>
        /// <param name="defaultValue">The default value of the parameter.</param>
        /// <param name="conditions">The conditions the parameter value must satisfy.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateRequiredParameter<T>(String identifier, String name, String remarks, String[] aliases, T defaultValue, params Predicate<Object>[] conditions)
        {
            return new GeometryStreamParameter(identifier, name, remarks, aliases, typeof(T), false, defaultValue, conditions);
        }

        /// <summary>
        /// Creates a required <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="type">The type declaration of the parameter.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateRequiredParameter(String identifier, String name, String remarks, Type type)
        {
            return new GeometryStreamParameter(identifier, name, remarks, null, type, false, null, null);
        }

        /// <summary>
        /// Creates a required <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="type">The type declaration of the parameter.</param>
        /// <param name="defaultValue">The default value of the parameter.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateRequiredParameter(String identifier, String name, String remarks, Type type, Object defaultValue)
        {
            return new GeometryStreamParameter(identifier, name, remarks, null, type, false, defaultValue, null);
        }

        /// <summary>
        /// Creates a required <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="aliases">The aliases.</param>
        /// <param name="type">The type declaration of the parameter.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateRequiredParameter(String identifier, String name, String remarks, String[] aliases, Type type)
        {
            return new GeometryStreamParameter(identifier, name, remarks, aliases, type, false, null, null);
        }

        /// <summary>
        /// Creates a required <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="aliases">The aliases.</param>
        /// <param name="defaultValue">The default value of the parameter.</param>
        /// <param name="type">The type declaration of the parameter.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateRequiredParameter(String identifier, String name, String remarks, String[] aliases, Type type, Object defaultValue)
        {
            return new GeometryStreamParameter(identifier, name, remarks, aliases, type, false, defaultValue, null);
        }

        /// <summary>
        /// Creates a required <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="type">The type declaration of the parameter.</param>
        /// <param name="conditions">The conditions the parameter value must satisfy.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateRequiredParameter(String identifier, String name, String remarks, Type type, params Predicate<Object>[] conditions)
        {
            return new GeometryStreamParameter(identifier, name, remarks, null, type, false, null, conditions);
        }

        /// <summary>
        /// Creates a required <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="type">The type declaration of the parameter.</param>
        /// <param name="defaultValue">The default value of the parameter.</param>
        /// <param name="conditions">The conditions the parameter value must satisfy.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateRequiredParameter(String identifier, String name, String remarks, Type type, Object defaultValue, params Predicate<Object>[] conditions)
        {
            return new GeometryStreamParameter(identifier, name, remarks, null, type, false, defaultValue, conditions);
        }

        /// <summary>
        /// Creates a required <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="aliases">The aliases.</param>
        /// <param name="type">The type declaration of the parameter.</param>
        /// <param name="conditions">The conditions the parameter value must satisfy.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateRequiredParameter(String identifier, String name, String remarks, String[] aliases, Type type, params Predicate<Object>[] conditions)
        {
            return new GeometryStreamParameter(identifier, name, remarks, aliases, type, false, null, conditions);
        }

        /// <summary>
        /// Creates a required <see cref="GeometryStreamParameter" />.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="aliases">The aliases.</param>
        /// <param name="type">The type declaration of the parameter.</param>
        /// <param name="defaultValue">The default value of the parameter.</param>
        /// <param name="conditions">The conditions the parameter value must satisfy.</param>
        /// <returns>The produced operation parameter.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The identifier is null.
        /// or
        /// The type is null.
        /// </exception>
        public static GeometryStreamParameter CreateRequiredParameter(String identifier, String name, String remarks, String[] aliases, Type type, Object defaultValue, params Predicate<Object>[] conditions)
        {
            return new GeometryStreamParameter(identifier, name, remarks, aliases, type, false, defaultValue, conditions);
        }

        #endregion
    }
}
