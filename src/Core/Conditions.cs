// <copyright file="Conditions.cs" company="Eötvös Loránd University (ELTE)">
//     Copyright (c) 2011-2019 Roberto Giachetta. Licensed under the
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

namespace AEGIS
{
    /// <summary>
    /// Defines a collection of conditions that can be applied to any value.
    /// </summary>
    /// <remarks>
    /// This type provides easy to use methods for declaring requirements for any value. 
    /// The predicates return <c>true</c> if the value satisfies the requirements; otherwise, <c>false</c>.
    /// The predicates never throw exception.
    /// </remarks>
    public static class Conditions
    {
        /// <summary>
        /// The value has no requirements.
        /// </summary>
        /// <returns>A <see cref="Predicate{Object}" /> that is always true.</returns>
        public static Predicate<Object> None()
        {
            return (value => true);
        }

        /// <summary>
        /// Requires that the value is positive.
        /// </summary>
        /// <returns>A <see cref="Predicate{Object}" /> that validates whether the value is positive.</returns>
        public static Predicate<Object> IsPositive()
        {
            return (value => (value is IConvertible) && Convert.ToDouble(value) > 0);
        }

        /// <summary>
        /// Requires that the value is negative.
        /// </summary>
        /// <returns>A <see cref="Predicate{Object}" /> that validates whether the value is negative.</returns>
        public static Predicate<Object> IsNegative()
        {
            return (value => (value is IConvertible) && Convert.ToDouble(value) < 0);
        }

        /// <summary>
        /// Requires that the value is not positive.
        /// </summary>
        /// <returns>A <see cref="Predicate{Object}" /> that validates whether the value is not positive.</returns>
        public static Predicate<Object> IsNotPositive()
        {
            return (value => (value is IConvertible) && Convert.ToDouble(value) <= 0);
        }

        /// <summary>
        /// Requires that the value is not negative.
        /// </summary>
        /// <returns>A <see cref="Predicate{Object}" /> that validates whether the value is not negative.</returns>
        public static Predicate<Object> IsNotNegative()
        {
            return (value => (value is IConvertible) && Convert.ToDouble(value) >= 0);
        }

        /// <summary>
        /// Requires that the value is even.
        /// </summary>
        /// <returns>A <see cref="Predicate{Object}" /> that validates whether the value is even.</returns>
        public static Predicate<Object> IsEven()
        {
            return (value => (value is IConvertible) && Convert.ToInt32(value) % 2 == 1);
        }

        /// <summary>
        /// Requires that the value is odd.
        /// </summary>
        /// <returns>A <see cref="Predicate{Object}" /> that validates whether the value is odd.</returns>
        public static Predicate<Object> IsOdd()
        {
            return (value => (value is IConvertible) && Convert.ToInt32(value) % 2 == 1);
        }

        /// <summary>
        /// Requires that the value is greater than or equal to the specified boundary.
        /// </summary>
        /// <param name="boundary">The boundary.</param>
        /// <returns>A <see cref="Predicate{Object}" /> that validates whether the value is greater than or equal to the specified boundary.</returns>
        public static Predicate<Object> IsGreaterThanOrEqualTo(Double boundary)
        {
            return (value => (value is IConvertible) && boundary <= Convert.ToDouble(value));
        }

        /// <summary>
        /// Requires that the value is greater than the specified boundary.
        /// </summary>
        /// <param name="boundary">The boundary.</param>
        /// <returns>A <see cref="Predicate{Object}" /> that validates whether the value is greater than the specified boundary.</returns>
        public static Predicate<Object> IsGreaterThan(Double boundary)
        {
            return (value => (value is IConvertible) && boundary < Convert.ToDouble(value));
        }

        /// <summary>
        /// Requires that the value is less than or equal to the specified boundary.
        /// </summary>
        /// <param name="boundary">The boundary.</param>
        /// <returns>A <see cref="Predicate{Object}" /> that validates whether the value is less than or equal to the specified boundary.</returns>
        public static Predicate<Object> IsLessThanOrEqualTo(Double boundary)
        {
            return (value => (value is IConvertible) && boundary >= Convert.ToDouble(value));
        }

        /// <summary>
        /// Requires that the value is less than the specified boundary.
        /// </summary>
        /// <param name="boundary">The boundary.</param>
        /// <returns>A <see cref="Predicate{Object}" /> that validates whether the value is less than the specified boundary.</returns>
        public static Predicate<Object> IsLessThan(Double boundary)
        {
            return (value => (value is IConvertible) && boundary > Convert.ToDouble(value));
        }

        /// <summary>
        /// Requires that the value is between the specified lower and upper boundaries.
        /// </summary>
        /// <param name="lowerBoundary">The lower boundary.</param>
        /// <param name="upperBoundary">The upper boundary.</param>
        /// <returns>A <see cref="Predicate{Object}" /> that validates whether the value is between the specified lower and upper boundaries.</returns>
        public static Predicate<Object> IsBetween(Double lowerBoundary, Double upperBoundary)
        {
            return (value => (value is IConvertible) && lowerBoundary <= Convert.ToDouble(value) && Convert.ToDouble(value) <= upperBoundary);
        }

        /// <summary>
        /// Requires that the value is strictly between the specified lower and upper boundaries.
        /// </summary>
        /// <param name="lowerBoundary">The lower boundary.</param>
        /// <param name="upperBoundary">The upper boundary.</param>
        /// <returns>A <see cref="Predicate{Object}" /> that validates whether the value is strictly between the specified lower and upper boundaries.</returns>
        public static Predicate<Object> IsStricklyBetween(Double lowerBoundary, Double upperBoundary)
        {
            return (value => (value is IConvertible) && lowerBoundary < Convert.ToDouble(value) && Convert.ToDouble(value) < upperBoundary);
        }

        /// <summary>
        /// Requires that the is one of the specified objects.
        /// </summary>
        /// <param name="objects">The objects.</param>
        /// <returns>A <see cref="Predicate{Object}" /> that validates whether the value is one of the specified objects.</returns>
        public static Predicate<Object> IsOneOf(params Object[] objects)
        {
            return (value => objects.Contains(value));
        }

        /// <summary>
        /// Requires that the is one of the specified objects.
        /// </summary>
        /// <param name="objects">The objects.</param>
        /// <returns>A <see cref="Predicate{Object}" /> that validates whether the value is one of the specified objects.</returns>
        public static Predicate<Object> IsOneOf(IEnumerable<Object> objects)
        {
            return (value => objects.Contains(value));
        }

        /// <summary>
        /// Requires that the object (or type) implements the specified interface.
        /// </summary>
        /// <typeparam name="InterfaceType">The interface type.</typeparam>
        /// <returns>A <see cref="Predicate{Object}" /> that validates that the object (or type) implements the specified interface.</returns>
        public static Predicate<Object> Implements<InterfaceType>()
        {
            return value => (value is Type) && ((value as Type).Equals(typeof(InterfaceType)) || (value as Type).GetInterfaces().Contains(typeof(InterfaceType)))
                            || (value is InterfaceType);
        }

        /// <summary>
        /// Requires that the object (or type) inherits the specified superclass type.
        /// </summary>
        /// <typeparam name="ClassType">The superclass type.</typeparam>
        /// <returns>A <see cref="Predicate{Object}" /> that validates that the object (or type) inherits the specified superclass type.</returns>
        public static Predicate<Object> Inherits<ClassType>()
        {
            return value => (value is Type) && ((value as Type).Equals(typeof(ClassType)) || (value as Type).IsSubclassOf(typeof(ClassType)) || (value as Type).GetInterfaces().Contains(typeof(ClassType)))
                            || (value is ClassType);
        }
    }
}
