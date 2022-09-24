using NUnit.Framework;
using Omtv.Api.Primitives;

namespace Omtv.Tests
{
    public class BorderTests
    {
        [Test]
        public void AllDataBorderSideTest()
        {
            Assert.That(BorderSide.Parse("3 white"), Is.EqualTo(new BorderSide(new Measure(3, Unit.Pixel), new ColorInfo(255, 255, 255, 255))));
        }
    }
}