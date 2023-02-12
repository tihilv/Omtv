using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Omtv.Engine;
using Omtv.Pdf;

namespace Omtv.Tests;

public class PdfTests
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
                      "<columns><column/><column width=\"50mm\"/><column/></columns>"+
                      "<row styles=\"odd\" height=\"50mm\"><cell align=\"center\" width=\"70%\" parents=\"borders\" foreColor=\"red\">v11\\nvvvvvvvvv</cell><cell rowSpan=\"2\" styles=\"borders\" valign=\"before\" align=\"center\" width=\"20px\">v12</cell><cell>v13</cell></row>" +
                      "<row><cell backColor=\"yellow\" align=\"center\" font=\"'Courier New' 14\">v21\\nmmmmmmmm</cell><cell font=\"'Times new roman' 20\">v23</cell></row>" +
                      "</table>" +
                      "<table name=\"bbb2\" border=\"5px\" border.right=\"7px blue\">" +
                      "<row styles=\"odd\" height=\"25px\"><cell align=\"center\" width=\"70%\" parents=\"borders\">q11</cell><cell rowSpan=\"2\" colSpan=\"2\" valign=\"after\" width=\"20px\">q12</cell></row>" +
                      "<row><cell align=\"center\">q21</cell></row>" +
                      "</table>" +
                      "</document>";

            await TableVisualizer.TransformAsync(doc, new PdfTableOutput(stream));
#if DEBUG
            await File.WriteAllBytesAsync("e:\\oo.pdf", stream.ToArray());
#endif
        }
    }
    
    [Test]
    public async Task FileTest()
    {
        using (var stream = new MemoryStream())
        {
            var doc = File.ReadAllText("e:\\1.txt");

            await TableVisualizer.TransformAsync(doc, new PdfTableOutput(stream));
#if DEBUG
            await File.WriteAllBytesAsync("e:\\oo.pdf", stream.ToArray());
#endif
        }
    }
}