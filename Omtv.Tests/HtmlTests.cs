using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Omtv.Engine;
using Omtv.Excel;
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
                                                     "<header width=\"297mm\" height=\"210mm\" margin.left=\"30mm\" name=\"Some name\">" +
                                                     "<style name=\"default\" backColor=\"white\" foreColor=\"black\"/>" +
                                                     "<style name=\"odd\" backColor=\"gray\"/>" +
                                                     "<style name=\"borders\" border=\"2px\"/>" +
                                                     "</header>" +
                                                     "<table name=\"Table 1\" border=\"5px\" border.right=\"7px blue\">" +
                                                     "<row styles=\"odd\" height=\"25px\"><cell align=\"center\" width=\"70%\" parents=\"borders\">v11</cell><cell rowSpan=\"2\" valign=\"after\" width=\"20px\">v12</cell><cell>v13</cell></row>" +
                                                     "<row><cell align=\"center\">v21</cell><cell>v23</cell></row>" +
                                                     "</table>" +
                                                     "</document>", new HtmlTableOutput(stream));

                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    var output = await reader.ReadToEndAsync();
#if DEBUG
                    await File.WriteAllTextAsync("e:\\oo.html", output);
#endif
                }
            }
        }
        
        [Test]
        public async Task ColumnWidthTest()
        {
            using (var stream = new MemoryStream())
            {
                await TableVisualizer.TransformAsync("<document>" +
                                                     "<header width=\"297mm\" height=\"210mm\" name=\"Some name\">" +
                                                     "<style name=\"default\" backColor=\"white\" foreColor=\"black\"/>" +
                                                     "</header>" +
                                                     "<table name=\"Table 1\" border=\"5px\" border.right=\"7px blue\">" +
                                                     "<row><cell width=\"100px\" border=\"1px\">v11</cell><cell colSpan=\"2\" width=\"100px\" border=\"1px\">v12</cell><cell width=\"100px\" border=\"1px\">v13</cell></row>" +
                                                     "<row><cell border=\"1px\">v21</cell><cell border=\"1px\">v22</cell><cell border=\"1px\">v23</cell><cell border=\"1px\">v24</cell></row>" +
                                                     "</table>" +
                                                     "</document>", new HtmlTableOutput(stream));

                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    var output = await reader.ReadToEndAsync();
#if DEBUG
                    await File.WriteAllTextAsync("e:\\ooo.html", output);
#endif
                }
            }
        }

        [Test]
        public async Task RealTest()
        {
            using (var stream = new MemoryStream())
            {
                await TableVisualizer.TransformAsync(File.ReadAllText("e:\\res2.qqq"), new ExcelTableOutput(stream));

                stream.Position = 0;
#if DEBUG
                await File.WriteAllBytesAsync("e:\\oo.xlsx", stream.ToArray());
#endif
            }
            
            using (var stream = new MemoryStream())
            {
                await TableVisualizer.TransformAsync(File.ReadAllText("e:\\res2.qqq"), new HtmlTableOutput(stream));

                stream.Position = 0;
#if DEBUG
                await File.WriteAllBytesAsync("e:\\oo.html", stream.ToArray());
#endif
            }
        }
    }
}