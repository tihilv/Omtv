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
#if DEBUG
        [Test]
        public async Task RealTest()
        {
            using (var stream = new MemoryStream())
            {
                await TableVisualizer.TransformAsync(File.ReadAllText("e:\\res2.qqq"), new ExcelTableOutput(stream));

                stream.Position = 0;

                await File.WriteAllBytesAsync("e:\\oo.xlsx", stream.ToArray());
            }
            
            using (var stream = new MemoryStream())
            {
                await TableVisualizer.TransformAsync(File.ReadAllText("e:\\res2.qqq"), new HtmlTableOutput(stream));

                stream.Position = 0;
                await File.WriteAllBytesAsync("e:\\oo.html", stream.ToArray());
            }
        }
        
    [Test]
    public async Task RegularTestWithChart()
    {
        using (var stream = new MemoryStream())
        {
            var doc = "<document>" +
                      "<header width=\"297mm\" height=\"210mm\" name=\"Some name\">" +
                      "<style name=\"default\" backColor=\"white\" foreColor=\"black\"/>" +
                      "<style name=\"odd\" backColor=\"gray\"/>" +
                      "<style name=\"borders\" border=\"1px\"/>" +
                      "</header>" +
                      "<table name=\"Table 1\" border=\"5px\" border.right=\"7px blue\">" +
                      "<columns><column/><column width=\"50mm\"/><column/></columns>"+
                      "<row styles=\"odd\" height=\"50mm\"><cell align=\"center\" width=\"70%\" parents=\"borders\" foreColor=\"red\">" +
                      "<chart type=\"Pie\" showLegend=\"true\" title=\"Market Share\"><categories>Company A, Company B, Company C</categories><series header=\"Share\" values=\"45, 35, 20\" /></chart>" +
                      "</cell><cell rowSpan=\"2\" styles=\"borders\" valign=\"before\" align=\"center\" width=\"20px\">" +
                      "<chart type=\"Line\" showLegend=\"true\" title=\"Some chart\"><categories>Jan, Feb, Mar</categories><series header=\"Company A\" values=\"45, 35, 20\" /><series header=\"Company B\" values=\"25, 45, 10\" /></chart>" +
                      "</cell><cell>v13</cell></row>" +
                      "<row><cell backColor=\"yellow\" align=\"center\" font=\"'Courier New' 14\">v21\\nmmmmmmmm</cell><cell font=\"'Times new roman' 20\">v23</cell></row>" +
                      "</table>" +
                      "<table name=\"bbb2\" border=\"5px\" border.right=\"7px blue\">" +
                      "<row styles=\"odd\" height=\"25px\"><cell align=\"center\" width=\"70%\" parents=\"borders\">q11</cell><cell rowSpan=\"2\" colSpan=\"2\" valign=\"after\" width=\"20px\">q12</cell></row>" +
                      "<row><cell align=\"center\">q21</cell></row>" +
                      "</table>" +
                      "</document>";

            await TableVisualizer.TransformAsync(doc, new HtmlTableOutput(stream));
#if DEBUG
            await File.WriteAllBytesAsync("e:\\ooC.html", stream.ToArray());
#endif
        }
    }
#endif
    }
}