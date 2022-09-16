using Omtv.Api.Primitives;

namespace Omtv.Tests;

public class FontTests
{
    [Test]
    public void RegularTest()
    {
        Assert.That(FontInfo.Parse("'Times new roman' 14"), Is.EqualTo(new FontInfo("Times new roman", new Measure(14, Unit.Em))));
    }
}