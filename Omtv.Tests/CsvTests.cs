using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Omtv.Csv;
using Omtv.Engine;

namespace Omtv.Tests
{

    public class CsvTests
    {
        [Test]
        public async Task RegularTest()
        {
            using (var stream = new MemoryStream())
            {
                await TableVisualizer.TransformAsync("<document><header width=\"297mm\" height=\"210mm\" name=\"Some name\"><style name=\"default\" backColor=\"white\"/><style name=\"odd\" backColor=\"gray\"/></header>" +
                                                     "<table name=\"Table 1\"><row><cell>v11</cell><cell rowSpan=\"2\">v12</cell><cell>v13</cell></row><row><cell>v21</cell><cell>v23</cell></row></table></document>", new CsvTableOutput(stream));

                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    var output = await reader.ReadToEndAsync();
                    Assert.That(output, Is.EqualTo(@"Table 1
v11;v12;v13
v21;;v23
"));
                }
            }
        }
    }
}