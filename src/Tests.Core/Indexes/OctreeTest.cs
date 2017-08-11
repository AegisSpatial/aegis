﻿// <copyright file="OctreeTest.cs" company="Eötvös Loránd University (ELTE)">
//     Copyright 2016-2017 Roberto Giachetta. Licensed under the
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

namespace AEGIS.Tests.Indexes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AEGIS.Geometries;
    using AEGIS.Indexes;
    using NUnit.Framework;
    using Shouldly;

    /// <summary>
    /// Test fixture for the <see cref="Octree" /> class.
    /// </summary>
    [TestFixture]
    public class OctreeTest
    {
        /// <summary>
        /// The geometry factory.
        /// </summary>
        private IGeometryFactory factory;

        /// <summary>
        /// The Octree.
        /// </summary>
        private Octree tree;

        /// <summary>
        /// Test setup.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.factory = new GeometryFactory();
            this.tree = new Octree(new Envelope(0, 100, 0, 100, 0, 100));
        }

        /// <summary>
        /// Tests the constructor.
        /// </summary>
        [Test]
        public void OctreeConstructorTest()
        {
            Octree tree = new Octree(new Envelope(0, 100, 0, 100, 0, 100));

            tree.NumberOfGeometries.ShouldBe(0);
            tree.IsReadOnly.ShouldBeFalse();
        }

        /// <summary>
        /// Tests the <see cref="Octree.Add" /> method.
        /// </summary>
        [Test]
        public void OctreeAddTest()
        {
            // Should add different types of a single geometry within the tree's bound
            this.tree.Add(this.factory.CreatePoint(5, 5, 5));
            this.tree.NumberOfGeometries.ShouldBe(1);

            Coordinate[] polygonCoordinates = new Coordinate[]
            {
                new Coordinate(0, 0, 0),
                new Coordinate(0, 10, 10),
                new Coordinate(10, 10, 10),
                new Coordinate(10, 0, 15),
                new Coordinate(0, 0, 0)
            };
            this.tree.Add(this.factory.CreatePolygon(polygonCoordinates));
            this.tree.NumberOfGeometries.ShouldBe(2);

            // Should add a single geometry from outside the tree's bound
            IPoint pointOutSideBound = this.factory.CreatePoint(110, 110, 50);
            this.tree.Add(pointOutSideBound);
            this.tree.NumberOfGeometries.ShouldBe(3);

            // Should add all geometries from geometry list
            List<IBasicGeometry> geometries = new List<IBasicGeometry>
            {
                this.factory.CreatePoint(65, 65),
                this.factory.CreatePolygon(
                    new Coordinate[]
                    {
                        new Coordinate(0, 0, 0),
                        new Coordinate(0, 15, 10),
                        new Coordinate(15, 15, 0),
                        new Coordinate(15, 0, 8),
                        new Coordinate(0, 0, 0)
                    })
            };
            this.tree.Add(geometries);
            this.tree.NumberOfGeometries.ShouldBe(5);

            // errors
            Should.Throw<ArgumentNullException>(() => this.tree.Add((IGeometry)null));
            Should.Throw<ArgumentNullException>(() => this.tree.Add((IEnumerable<IGeometry>)null));
        }

        /// <summary>
        /// Tests the <see cref="Octree.Contains" /> method.
        /// </summary>
        [Test]
        public void OctreeContainsTest()
        {
            List<IPoint> points = new List<IPoint>(Enumerable.Range(1, 99).Select(value => this.factory.CreatePoint(value, value, value)));
            this.tree.Add(points);

            this.tree.Contains(null).ShouldBeFalse();
            this.tree.Contains(this.factory.CreatePoint(1000, 1000, 1000)).ShouldBeFalse();
            this.tree.Contains(this.factory.CreatePoint(Coordinate.Undefined)).ShouldBeFalse();

            foreach (IPoint point in points)
                this.tree.Contains(point).ShouldBeTrue();
        }

        /// <summary>
        /// Tests the <see cref="Octree.Search" /> method.
        /// </summary>
        [Test]
        public void OctreeSearchTest()
        {
            List<IPoint> points = new List<IPoint>(Enumerable.Range(1, 99).Select(value => this.factory.CreatePoint(value, value, value)));
            this.tree.Add(points);

            // Should be empty for undefined envelope
            this.tree.Search(Envelope.Undefined).ShouldBeEmpty();

            // Should be empty for envelopes that do not contain any of the geometries
            this.tree.Search(new Envelope(0.5, 0.6, 0.5, 0.6, 0.5, 0.6));
            this.tree.Search(new Envelope(100, 100, 100, 100, 100, 100));
            this.tree.Search(new Envelope(200, 210, 200, 210, 200, 210));

            // Should find all geometries as results for these searches
            this.tree.Search(Envelope.Infinity).Count().ShouldBe(this.tree.NumberOfGeometries);
            this.tree.Search(Envelope.FromEnvelopes(points.Select(geometry => geometry.Envelope))).Count().ShouldBe(this.tree.NumberOfGeometries);

            // Should find correct results for some concrete examples
            QuadTree tree = new QuadTree(new Envelope(0, 100, 0, 100, 0, 100));

            Coordinate[] firstShell = new Coordinate[]
            {
                new Coordinate(0, 0, 0),
                new Coordinate(0, 10, 10),
                new Coordinate(10, 10, 10),
                new Coordinate(10, 0, 0),
                new Coordinate(0, 0, 0)
            };

            Coordinate[] secondShell = new Coordinate[]
            {
                new Coordinate(0, 0, 0),
                new Coordinate(0, 15, 15),
                new Coordinate(15, 15, 15),
                new Coordinate(15, 0, 0),
                new Coordinate(0, 0, 0)
            };

            Coordinate[] thirdShell = new Coordinate[]
            {
                new Coordinate(30, 30, 0),
                new Coordinate(30, 40, 40),
                new Coordinate(40, 40, 40),
                new Coordinate(40, 30, 50),
                new Coordinate(30, 30, 0)
            };

            tree.Add(this.factory.CreatePolygon(firstShell));
            tree.Add(this.factory.CreatePolygon(secondShell));
            tree.Add(this.factory.CreatePolygon(thirdShell));

            tree.Search(Envelope.FromCoordinates(firstShell)).Count().ShouldBe(1);
            tree.Search(Envelope.FromCoordinates(secondShell)).Count().ShouldBe(2);
            tree.Search(Envelope.FromCoordinates(thirdShell)).Count().ShouldBe(1);
            tree.Search(Envelope.FromCoordinates(secondShell.Union(thirdShell))).Count().ShouldBe(3);

            // Should throw exception for null
            Should.Throw<ArgumentNullException>(() => this.tree.Search(null));
        }

        /// <summary>
        /// Tests the <see cref="Octree.Remove" /> method.
        /// </summary>
        [Test]
        public void OctreeRemoveTest()
        {
            List<IPoint> points = new List<IPoint>(Enumerable.Range(1, 99).Select(value => this.factory.CreatePoint(value, value, value)));
            this.tree.Add(points);

            // Should not remove geometries that are not in the tree
            this.tree.Remove(this.factory.CreatePoint(1000, 1000, 1000)).ShouldBeFalse();
            this.tree.Remove(this.factory.CreatePoint(Coordinate.Undefined)).ShouldBeFalse();
            this.tree.Remove(this.factory.CreatePoint(1.5, 1.5, 0)).ShouldBeFalse();

            // Should remove geometries that are in the tree
            foreach (IPoint point in points)
                this.tree.Remove(point).ShouldBeTrue();

            this.tree.NumberOfGeometries.ShouldBe(0);

            // Should remove correctly based on envelope
            this.tree.Add(points);

            this.tree.Remove(new Envelope(0, 49, 0, 49, 0, 49)).ShouldBeTrue();
            this.tree.NumberOfGeometries.ShouldBe(50);
            this.tree.Remove(Envelope.Infinity).ShouldBeTrue();
            this.tree.NumberOfGeometries.ShouldBe(0);

            // Should remove based on envelope with results
            this.tree.Remove(Envelope.Infinity, out List<IBasicGeometry> geometries).ShouldBeFalse();
            geometries.Count.ShouldBe(0);

            this.tree.Add(points);

            this.tree.Remove(Envelope.Infinity, out geometries).ShouldBeTrue();
            geometries.Count.ShouldBe(points.Count);

            // Should throw exception when removing with null
            Should.Throw<ArgumentNullException>(() => this.tree.Remove((Envelope)null));
            Should.Throw<ArgumentNullException>(() => this.tree.Remove((IGeometry)null));
        }

        /// <summary>
        /// Tests the <see cref="Octree.Clear" /> method.
        /// </summary>
        [Test]
        public void OctreeClearTest()
        {
            List<IPoint> points = new List<IPoint>(Enumerable.Range(1, 99).Select(value => this.factory.CreatePoint(value, value, value)));
            this.tree.Add(points);

            this.tree.Clear();

            this.tree.NumberOfGeometries.ShouldBe(0);
        }
    }
}