﻿// <copyright file="GaussianRandomGenerator.cs" company="Eötvös Loránd University (ELTE)">
//     Copyright 2016 Roberto Giachetta. Licensed under the
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

namespace ELTE.AEGIS.Numerics.Randomizers
{
    using System;
    using ELTE.AEGIS.Numerics.Resources;

    /// <summary>
    /// Represents a random number generator using a Gaussian (normal) distribution.
    /// </summary>
    /// <remarks>
    /// This random generator uses the polar form of the Box-Muller transformation to create values with Gaussian distribution from values generated by the specified random generator.
    /// If no random generator is specified, the default <see cref="Random" /> is used.
    /// </remarks>
    public class GaussianRandomGenerator : Random
    {
        #region Private fields

        /// <summary>
        /// A value indicating whether the next number is available.
        /// </summary>
        private Boolean available;

        /// <summary>
        /// The next generated number.
        /// </summary>
        private Double nextGauss;

        /// <summary>
        /// The underlying random generator.
        /// </summary>
        private Random random;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GaussianRandomGenerator" /> class.
        /// </summary>
        public GaussianRandomGenerator()
            : this(new Random())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GaussianRandomGenerator" /> class.
        /// </summary>
        /// <param name="random">The underlying random generator.</param>
        /// <exception cref="System.ArgumentNullException">The random generator is null.</exception>
        public GaussianRandomGenerator(Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random), Messages.RandomIsNull);

            this.random = random;
        }

        #endregion

        #region Public Random methods

        /// <summary>
        /// Returns a non-negative random integer.
        /// </summary>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than <see cref="Int32.MaxValue" />.</returns>
        public override Int32 Next()
        {
            return (Int32)(this.Sample() * ((Int64)Int32.MaxValue + 1));
        }

        /// <summary>
        /// Returns a random integer that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to <paramref name="minValue" /> and less than <paramref name="maxValue" />; that is, the range of return values includes <paramref name="minValue" /> but not <paramref name="maxValue" />. If <paramref name="minValue" /> equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">The minimum value is greater than the maximum value.</exception>
        public override Int32 Next(Int32 minValue, Int32 maxValue)
        {
            if (maxValue < minValue)
                throw new ArgumentOutOfRangeException(nameof(minValue), Messages.MinValueGreaterThanMaxValue);

            if (maxValue == minValue)
                return minValue;

            Int64 range = (Int64)maxValue - minValue;

            return (Int32)(this.Sample() * range + minValue);
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">An array of bytes to contain random numbers.</param>
        /// <exception cref="System.ArgumentNullException">The buffer is null.</exception>
        public override void NextBytes(Byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), Messages.BufferIsNull);

            for (Int32 i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (Byte)(this.Sample() * (Byte.MaxValue + 1));
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Returns a random number based on the median and standard deviation.
        /// </summary>
        /// <param name="median">The median.</param>
        /// <param name="standardDeviation">The standard deviation.</param>
        /// <returns>The generated number.</returns>
        public Double NextDouble(Double median, Double standardDeviation)
        {
            return median + standardDeviation * this.InternalSample();
        }

        /// <summary>
        /// Returns a random number based on the standard deviation.
        /// </summary>
        /// <param name="standardDeviation">The standard deviation.</param>
        /// <returns>The generated number.</returns>
        public Double NextDouble(Double standardDeviation)
        {
            return standardDeviation * this.InternalSample();
        }

        #endregion

        #region Protected Random methods

        /// <summary>
        /// Returns a random floating-point number between 0.0 and 1.0.
        /// </summary>
        /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        protected override Double Sample()
        {
            Double sample;
            do
            {
                sample = (this.InternalSample() + Math.PI / 2) / Math.PI;
            }
            while (sample < 0 || sample >= 1);

            return sample;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Returns a random floating-point number between -1.0 and 1.0.
        /// </summary>
        /// <returns>A double-precision floating point number that is greater than or equal to -1.0, and less than 1.0.</returns>
        private Double InternalSample()
        {
            if (this.available)
            {
                this.available = false;
                return this.nextGauss;
            }

            Double u, v, s;
            do
            {
                u = 2 * this.random.NextDouble() - 1;
                v = 2 * this.random.NextDouble() - 1;

                s = u * u + v * v;
            }
            while (s == 0 || s >= 1);

            s = Math.Sqrt(-2 * Math.Log(s) / s);

            this.nextGauss = v * s;
            this.available = true;

            return u * s;
        }

        #endregion
    }
}