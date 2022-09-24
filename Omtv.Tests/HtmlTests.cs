using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Omtv.Engine;
using Omtv.Html;

namespace Omtv.Tests
{
    public class HtmlTests
    {
        [Test]
        public async Task RegularTest()
        {
            using (var stream = new MemoryStream())
            {
                await TableVisualizer.TransformAsync("<document>" +
                                                     "<header width=\"297mm\" height=\"210mm\" name=\"Some name\">" +
                                                     "<style name=\"default\" backColor=\"white\" foreColor=\"black\"/><style name=\"odd\" backColor=\"gray\"/>" +
                                                     "<style name=\"borders\" border=\"2px\"/>" +
                                                     "</header>" +
                                                     "<table name=\"Table 1\" border=\"5px\" border.right=\"7px blue\">" +
                                                     "<row height=\"25px\"><cell align=\"center\" width=\"70%\" parents=\"borders\">v11</cell><cell rowSpan=\"2\" valign=\"after\" width=\"20px\">v12</cell><cell>v13</cell></row>" +
                                                     "<row><cell align=\"center\">v21</cell><cell>v23</cell></row>" +
                                                     "</table>" +
                                                     "</document>", new HtmlTableOutput(stream));

                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    var output = await reader.ReadToEndAsync();
                    //await File.WriteAllTextAsync("e:\\oo.html", output);
                }
            }
        }
    }
}