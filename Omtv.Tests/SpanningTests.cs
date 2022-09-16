using Omtv.Api.Primitives;

namespace Omtv.Tests;

public class SpanningTests
{
    [Test]
    public void HexTest()
    {
        Assert.That(ColorInfo.Parse("#abcdef"), Is.EqualTo(new ColorInfo(255, 171, 205, 239)));
    }
}