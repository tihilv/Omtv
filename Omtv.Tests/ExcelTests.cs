using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Omtv.Engine;
using Omtv.Excel;

namespace Omtv.Tests;

public class ExcelTests
{
    [Test]
    public async Task RegularTest()
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
                      "<columns><column/><column width=\"50mm\"/></columns>"+
                      "<row styles=\"odd\" height=\"50mm\"><cell align=\"center\" width=\"70%\" parents=\"borders\" foreColor=\"red\">v11</cell><cell rowSpan=\"2\" styles=\"borders\" valign=\"before\" align=\"center\" width=\"20px\">v12</cell><cell>v13</cell></row>" +
                      "<row><cell backColor=\"yellow\" align=\"center\" font=\"'Courier New' 14\">v21</cell><cell font=\"'Times new roman' 20\">v23</cell></row>" +
                      "</table>" +
                      "<table name=\"bbb2\" border=\"5px\" border.right=\"7px blue\">" +
                      "<row styles=\"odd\" height=\"25px\"><cell align=\"center\" width=\"70%\" parents=\"borders\">q11</cell><cell rowSpan=\"2\" colSpan=\"2\" valign=\"after\" width=\"20px\">q12</cell></row>" +
                      "<row><cell align=\"center\">q21</cell></row>" +
                      "</table>" +
                      "</document>";

            await TableVisualizer.TransformAsync(doc, new ExcelTableOutput(stream)); 
            await File.WriteAllBytesAsync("e:\\oo.xlsx", stream.ToArray());
        }
    }
/*
        [Test]
        public async Task RegularTest2()
        {
            using (var stream = new MemoryStream())
            {
                await TableVisualizer.TransformAsync(File.ReadAllText("f:\\text.txt"), new HtmlTableOutput(stream));

                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    var output = await reader.ReadToEndAsync();
                    //await File.WriteAllTextAsync("e:\\oo.html", output);
                }
            }
        }
*/
}