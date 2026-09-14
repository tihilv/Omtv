using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using NUnit.Framework;
using Omtv.Api.Model;
using Omtv.Api.Primitives;
using Omtv.Engine;
using Omtv.Excel;
using Omtv.Html;
using Omtv.Pdf;
using PdfSharpCore.Pdf.IO;
using Chart = Omtv.Api.Model.Chart;

namespace Omtv.Tests
{
    [TestFixture]
    public class ChartTests
    {
        private const String SampleBarChartXml = @"
<document>
    <header width=""297mm"" height=""210mm"" margin.left=""10mm"" margin.right=""10mm"" margin.top=""10mm"" margin.bottom=""10mm"">
        <style name=""default"" backColor=""white"" foreColor=""black"" />
    </header>
    <table name=""Sales Report"">
        <columns>
            <column width=""50%"" />
            <column width=""50%"" />
        </columns>
        <row height=""80mm"">
            <cell>
                <chart type=""Bar"" showLegend=""true"" title=""Q1-Q3 Sales"">
                    <categories>Q1, Q2, Q3</categories>
                    <series header=""Product A"" color=""#4F81BD"" values=""100, 150, 120"" />
                    <series header=""Product B"" color=""#C0504D"" values=""80, 90, 110"" />
                </chart>
            </cell>
            <cell>
                <chart type=""Line"" legend=""true"" title=""Monthly Growth"">
                    <category>Jan</category>
                    <category>Feb</category>
                    <category>Mar</category>
                    <series header=""Growth Rate"" color=""#9BBB59"">
                        <value>5.2</value>
                        <value>7.8</value>
                        <value>12.1</value>
                    </series>
                </chart>
            </cell>
        </row>
        <row height=""80mm"">
            <cell colSpan=""2"">
                <chart type=""Pie"" showLegend=""true"" title=""Market Share"">
                    <categories>Company A, Company B, Company C</categories>
                    <series header=""Share"" values=""45, 35, 20"" />
                </chart>
            </cell>
        </row>
    </table>
</document>";

        [Test]
        public void ChartModelTest()
        {
            var chart = new Chart(ChartType.Line, true)
            {
                Title = "Performance"
            };
            chart.Categories.Add("2021");
            chart.Categories.Add("2022");

            var series1 = new ChartSeries("Revenue", ColorInfo.Parse("#FF0000"), new[] { 100.0, 200.0 });
            var series2 = new ChartSeries("Cost", ColorInfo.Parse("#0000FF"), new[] { 80.0, 120.0 });

            chart.Series.Add(series1);
            chart.Series.Add(series2);

            Assert.AreEqual(ChartType.Line, chart.Type);
            Assert.IsTrue(chart.ShowLegend);
            Assert.AreEqual("Performance", chart.Title);
            Assert.AreEqual(2, chart.Categories.Count);
            Assert.AreEqual(2, chart.Series.Count);
            Assert.AreEqual("Revenue", chart.Series[0].Header);
            Assert.AreEqual(2, chart.Series[0].Values.Count);
            Assert.AreEqual(100.0, chart.Series[0].Values[0]);
            Assert.AreEqual(200.0, chart.Series[0].Values[1]);
        }

        [Test]
        public async Task HtmlChartOutputTest()
        {
            using var stream = new MemoryStream();
            var output = new HtmlTableOutput(stream);

            await TableVisualizer.TransformAsync(SampleBarChartXml, output);

            stream.Position = 0;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var html = await reader.ReadToEndAsync();

            Assert.IsNotEmpty(html);
            Assert.IsTrue(html.Contains("<svg"));
            Assert.IsTrue(html.Contains("</svg>"));
            Assert.IsTrue(html.Contains("Q1-Q3 Sales"));
            Assert.IsTrue(html.Contains("Product A"));
            Assert.IsTrue(html.Contains("Product B"));
            Assert.IsTrue(html.Contains("#4F81BD") || html.Contains("#4f81bd"));
            Assert.IsTrue(html.Contains("Monthly Growth"));
            Assert.IsTrue(html.Contains("Market Share"));
            Assert.IsTrue(html.Contains("<polyline") || html.Contains("<path"));
        }

        [Test]
        public async Task PdfChartOutputTest()
        {
            using var stream = new MemoryStream();
            var output = new PdfTableOutput(stream);

            await TableVisualizer.TransformAsync(SampleBarChartXml, output);

            stream.Position = 0;
            var pdfDocument = PdfReader.Open(stream, PdfDocumentOpenMode.ReadOnly);

            Assert.IsNotNull(pdfDocument);
            Assert.GreaterOrEqual(pdfDocument.PageCount, 1);
        }

        [Test]
        public async Task ExcelChartOutputTest()
        {
            using var stream = new MemoryStream();
            var output = new ExcelTableOutput(stream);

            await TableVisualizer.TransformAsync(SampleBarChartXml, output);

            stream.Position = 0;
            using var spreadsheet = SpreadsheetDocument.Open(stream, false);
            var workbookPart = spreadsheet.WorkbookPart;
            Assert.IsNotNull(workbookPart);

            var worksheetPart = workbookPart.WorksheetParts.First();
            Assert.IsNotNull(worksheetPart);
            Assert.IsNotNull(worksheetPart.DrawingsPart);

            var drawingsPart = worksheetPart.DrawingsPart;
            Assert.IsNotNull(drawingsPart.WorksheetDrawing);

            var anchors = drawingsPart.WorksheetDrawing.Elements<TwoCellAnchor>().ToList();
            Assert.AreEqual(3, anchors.Count);

            var chartParts = drawingsPart.ChartParts.ToList();
            Assert.AreEqual(3, chartParts.Count);

            // Verify BarChart
            var barChartSpace = chartParts[0].ChartSpace;
            Assert.IsNotNull(barChartSpace);
            var barChart = barChartSpace.Descendants<BarChart>().FirstOrDefault();
            Assert.IsNotNull(barChart);
            var barSeriesList = barChart.Elements<BarChartSeries>().ToList();
            Assert.AreEqual(2, barSeriesList.Count);

            // Verify LineChart
            var lineChartSpace = chartParts[1].ChartSpace;
            Assert.IsNotNull(lineChartSpace);
            var lineChart = lineChartSpace.Descendants<LineChart>().FirstOrDefault();
            Assert.IsNotNull(lineChart);

            // Verify PieChart
            var pieChartSpace = chartParts[2].ChartSpace;
            Assert.IsNotNull(pieChartSpace);
            var pieChart = pieChartSpace.Descendants<PieChart>().FirstOrDefault();
            Assert.IsNotNull(pieChart);

            var validator = new OpenXmlValidator();
            var errors = validator.Validate(spreadsheet).ToList();
            Assert.IsEmpty(errors, string.Join("\n", errors.Select(e => $"[{e.Part?.Uri}] ({e.Path?.XPath}) {e.Description}")));
        }

        [Test]
        public async Task MultiSeriesPieAndBarTest()
        {
            var xml = @"
<document>
    <header width=""297mm"" height=""210mm"" margin.left=""10mm"" margin.right=""10mm"" margin.top=""10mm"" margin.bottom=""10mm"">
        <style name=""default"" backColor=""white"" foreColor=""black"" />
    </header>
    <table>
        <columns><column width=""100%"" /></columns>
        <row height=""100mm"">
            <cell>
                <chart type=""Pie"" legend=""true"" categories=""Alpha, Beta, Gamma"">
                    <series header=""S1"" color=""#FF0000"" values=""10, 20, 30"" />
                </chart>
            </cell>
        </row>
    </table>
</document>";

            using var htmlStream = new MemoryStream();
            await TableVisualizer.TransformAsync(xml, new HtmlTableOutput(htmlStream));
            htmlStream.Position = 0;
            var html = new StreamReader(htmlStream).ReadToEnd();
            Assert.IsTrue(html.Contains("Alpha"));
            Assert.IsTrue(html.Contains("Beta"));
            Assert.IsTrue(html.Contains("Gamma"));

            using var pdfStream = new MemoryStream();
            await TableVisualizer.TransformAsync(xml, new PdfTableOutput(pdfStream));
            pdfStream.Position = 0;
            var pdfDoc = PdfReader.Open(pdfStream, PdfDocumentOpenMode.ReadOnly);
            Assert.AreEqual(1, pdfDoc.PageCount);

            using var excelStream = new MemoryStream();
            await TableVisualizer.TransformAsync(xml, new ExcelTableOutput(excelStream));
            excelStream.Position = 0;
            using var excelDoc = SpreadsheetDocument.Open(excelStream, false);
            var wsPart = excelDoc.WorkbookPart!.WorksheetParts.First();
            Assert.IsNotNull(wsPart.DrawingsPart);
            Assert.IsNotNull(wsPart.DrawingsPart.ChartParts.First().ChartSpace.Descendants<PieChart>().FirstOrDefault());

            var validator = new OpenXmlValidator();
            var errors = validator.Validate(excelDoc).ToList();
            Assert.IsEmpty(errors, string.Join("\n", errors.Select(e => $"[{e.Part?.Uri}] ({e.Path?.XPath}) {e.Description}")));
        }

        [Test]
        public async Task ManySeriesWithoutLegendTest()
        {
            var xml = @"
<document>
    <header width=""297mm"" height=""210mm"" margin.left=""10mm"" margin.right=""10mm"" margin.top=""10mm"" margin.bottom=""10mm"">
        <style name=""default"" backColor=""white"" foreColor=""black"" />
    </header>
    <table>
        <columns><column width=""100%"" /></columns>
        <row height=""100mm"">
            <cell>
                <chart type=""Line"" showLegend=""false"">
                    <series header=""S1"" color=""#111111"" values=""1, 2, 3"" />
                    <series header=""S2"" color=""#222222"" values=""2, 3, 4"" />
                    <series header=""S3"" color=""#333333"" values=""3, 4, 5"" />
                    <series header=""S4"" color=""#444444"" values=""4, 5, 6"" />
                    <series header=""S5"" color=""#555555"" values=""5, 6, 7"" />
                </chart>
            </cell>
        </row>
    </table>
</document>";

            using var htmlStream = new MemoryStream();
            await TableVisualizer.TransformAsync(xml, new HtmlTableOutput(htmlStream));
            htmlStream.Position = 0;
            var html = new StreamReader(htmlStream).ReadToEnd();
            Assert.IsTrue(html.Contains("<polyline"));

            using var pdfStream = new MemoryStream();
            await TableVisualizer.TransformAsync(xml, new PdfTableOutput(pdfStream));
            pdfStream.Position = 0;
            var pdfDoc = PdfReader.Open(pdfStream, PdfDocumentOpenMode.ReadOnly);
            Assert.AreEqual(1, pdfDoc.PageCount);

            using var excelStream = new MemoryStream();
            await TableVisualizer.TransformAsync(xml, new ExcelTableOutput(excelStream));
            excelStream.Position = 0;
            using var excelDoc = SpreadsheetDocument.Open(excelStream, false);
            var wsPart = excelDoc.WorkbookPart!.WorksheetParts.First();
            var lineChart = wsPart.DrawingsPart!.ChartParts.First().ChartSpace.Descendants<LineChart>().FirstOrDefault();
            Assert.IsNotNull(lineChart);
            Assert.AreEqual(5, lineChart!.Elements<LineChartSeries>().Count());

            var validator = new OpenXmlValidator();
            var errors = validator.Validate(excelDoc).ToList();
            Assert.IsEmpty(errors, string.Join("\n", errors.Select(e => $"[{e.Part?.Uri}] ({e.Path?.XPath}) {e.Description}")));
        }

        [Test]
        public async Task SpannedCellWithChartTest()
        {
            var xml = @"
<document>
    <header width=""297mm"" height=""210mm"" margin.left=""10mm"" margin.right=""10mm"" margin.top=""10mm"" margin.bottom=""10mm"">
        <style name=""default"" backColor=""white"" foreColor=""black"" />
    </header>
    <table>
        <columns>
            <column width=""30%"" />
            <column width=""70%"" />
        </columns>
        <row height=""40mm"">
            <cell>Summary Data</cell>
            <cell rowSpan=""2"">
                <chart type=""Bar"" showLegend=""true"" title=""Spanned Chart"">
                    <categories>Cat 1, Cat 2</categories>
                    <series header=""Series 1"" color=""#00FF00"" values=""50, 100"" />
                </chart>
            </cell>
        </row>
        <row height=""40mm"">
            <cell>Details</cell>
        </row>
    </table>
</document>";

            using var htmlStream = new MemoryStream();
            await TableVisualizer.TransformAsync(xml, new HtmlTableOutput(htmlStream));
            htmlStream.Position = 0;
            var html = new StreamReader(htmlStream).ReadToEnd();
            Assert.IsTrue(html.Contains("rowspan=\"2\""));
            Assert.IsTrue(html.Contains("Spanned Chart"));

            using var pdfStream = new MemoryStream();
            await TableVisualizer.TransformAsync(xml, new PdfTableOutput(pdfStream));
            pdfStream.Position = 0;
            var pdfDoc = PdfReader.Open(pdfStream, PdfDocumentOpenMode.ReadOnly);
            Assert.AreEqual(1, pdfDoc.PageCount);

            using var excelStream = new MemoryStream();
            await TableVisualizer.TransformAsync(xml, new ExcelTableOutput(excelStream));
            excelStream.Position = 0;
            using var excelDoc = SpreadsheetDocument.Open(excelStream, false);
            var wsPart = excelDoc.WorkbookPart!.WorksheetParts.First();
            var anchor = wsPart.DrawingsPart!.WorksheetDrawing.Elements<TwoCellAnchor>().First();
            Assert.IsNotNull(anchor);
            Assert.AreEqual("1", anchor.FromMarker.ColumnId.Text);
            Assert.AreEqual("0", anchor.FromMarker.RowId.Text);
            Assert.AreEqual("2", anchor.ToMarker.ColumnId.Text);
            Assert.AreEqual("2", anchor.ToMarker.RowId.Text);

            var validator = new OpenXmlValidator();
            var errors = validator.Validate(excelDoc).ToList();
            Assert.IsEmpty(errors, string.Join("\n", errors.Select(e => $"[{e.Part?.Uri}] ({e.Path?.XPath}) {e.Description}")));
        }
    }
}
