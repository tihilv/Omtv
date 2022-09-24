using System;
using NUnit.Framework;
using Omtv.Api.Primitives;

namespace Omtv.Tests
{
    public class MeasureTests
    {
        [Test]
        public void EmTest()
        {
            Assert.That(Measure.Parse("11.5em"), Is.EqualTo(new Measure(11.5, Unit.Em)));
        }

        [Test]
        public void PxTest()
        {
            Assert.That(Measure.Parse("12.5px"), Is.EqualTo(new Measure(12.5, Unit.Pixel)));
        }

        [Test]
        public void PercentTest()
        {
            Assert.That(Measure.Parse("13.5%"), Is.EqualTo(new Measure(13.5, Unit.Percent)));
        }

        [Test]
        public void MmTest()
        {
            Assert.That(Measure.Parse("13.5mm"), Is.EqualTo(new Measure(13.5, Unit.Mm)));
        }

        [Test]
        public void SpacingTest()
        {
            Assert.That(Measure.Parse("   13.5   mm   "), Is.EqualTo(new Measure(13.5, Unit.Mm)));
        }

        [Test]
        public void DefaultSuccessTest()
        {
            Assert.That(Measure.Parse("13.5", Unit.Percent), Is.EqualTo(new Measure(13.5, Unit.Percent)));
        }

        [Test]
        public void DefaultFailTest()
        {
            try
            {
                Measure.Parse("13.5");
                Assert.Fail();
            }
            catch (FormatException)
            {
                // ok
            }
        }
    }
}