using System;
using NUnit.Framework;
using Omtv.Api.Primitives;

namespace Omtv.Tests
{
    public class ColorTests
    {
        [Test]
        public void HexTest()
        {
            Assert.That(ColorInfo.Parse("#abcdef"), Is.EqualTo(new ColorInfo(255, 171, 205, 239)));
        }

        [Test]
        public void NameTest()
        {
            Assert.That(ColorInfo.Parse("red"), Is.EqualTo(new ColorInfo(255, 255, 0, 0)));
        }

        [Test]
        public void FailTest()
        {
            try
            {
                ColorInfo.Parse("qwerty");
                Assert.Fail();
            }
            catch (FormatException)
            {
                // ok
            }
        }
    }
}