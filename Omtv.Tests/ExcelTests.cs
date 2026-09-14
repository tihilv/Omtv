using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
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
#if DEBUG
            try
            {
                await File.WriteAllBytesAsync("e:\\oo.xlsx", stream.ToArray());
            }
            catch (IOException) { }
#endif
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
                      "<columns><column/><column width=\"50mm\"/><column/></columns>" +
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

            await TableVisualizer.TransformAsync(doc, new ExcelTableOutput(stream));

            stream.Position = 0;
            using var docPackage = SpreadsheetDocument.Open(stream, false);
            var validator = new OpenXmlValidator();
            var errors = validator.Validate(docPackage).ToList();
            var errorMessages = string.Join("\n", errors.Select(e => $"[{e.Part?.Uri}] ({e.Path?.XPath}) {e.Description}"));
            Assert.IsEmpty(errors, errorMessages);

#if DEBUG
            try
            {
                await File.WriteAllBytesAsync("e:\\ooC.xlsx", stream.ToArray());
            }
            catch (IOException) { }
#endif
        }
    }

}