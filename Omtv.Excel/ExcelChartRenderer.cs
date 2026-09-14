using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using Omtv.Api.Model;
using Omtv.Api.Primitives;
using A = DocumentFormat.OpenXml.Drawing;
using C = DocumentFormat.OpenXml.Drawing.Charts;
using S = DocumentFormat.OpenXml.Spreadsheet;
using Xdr = DocumentFormat.OpenXml.Drawing.Spreadsheet;

namespace Omtv.Excel
{
    internal static class ExcelChartRenderer
    {
        public static void AddChartToWorksheet(WorksheetPart worksheetPart, Chart chart, Int32 rowIndex, Int32 colIndex, Int32 rowSpan, Int32 colSpan, Int32 chartId)
        {
            var drawingsPart = worksheetPart.DrawingsPart ?? worksheetPart.AddNewPart<DrawingsPart>();

            if (drawingsPart.WorksheetDrawing == null)
            {
                drawingsPart.WorksheetDrawing = new Xdr.WorksheetDrawing();
            }

            var chartPart = drawingsPart.AddNewPart<ChartPart>();
            chartPart.ChartSpace = CreateChartSpace(chart);
            chartPart.ChartSpace.Save();

            var chartPartId = drawingsPart.GetIdOfPart(chartPart);

            var fromCol = Math.Max(0, colIndex - 1);
            var fromRow = Math.Max(0, rowIndex - 1);
            var toCol = fromCol + Math.Max(1, colSpan);
            var toRow = fromRow + Math.Max(1, rowSpan);

            var fromMarker = new Xdr.FromMarker
            {
                ColumnId = new Xdr.ColumnId(fromCol.ToString()),
                ColumnOffset = new Xdr.ColumnOffset("0"),
                RowId = new Xdr.RowId(fromRow.ToString()),
                RowOffset = new Xdr.RowOffset("0")
            };

            var toMarker = new Xdr.ToMarker
            {
                ColumnId = new Xdr.ColumnId(toCol.ToString()),
                ColumnOffset = new Xdr.ColumnOffset("0"),
                RowId = new Xdr.RowId(toRow.ToString()),
                RowOffset = new Xdr.RowOffset("0")
            };

            var graphicFrame = new Xdr.GraphicFrame
            {
                NonVisualGraphicFrameProperties = new Xdr.NonVisualGraphicFrameProperties(
                    new Xdr.NonVisualDrawingProperties { Id = (UInt32)chartId, Name = $"Chart {chartId}" },
                    new Xdr.NonVisualGraphicFrameDrawingProperties()
                ),
                Transform = new Xdr.Transform(
                    new A.Offset { X = 0, Y = 0 },
                    new A.Extents { Cx = 0, Cy = 0 }
                ),
                Graphic = new A.Graphic(
                    new A.GraphicData(
                        new C.ChartReference { Id = chartPartId }
                    ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/chart" }
                )
            };

            var twoCellAnchor = new Xdr.TwoCellAnchor(
                fromMarker,
                toMarker,
                graphicFrame,
                new Xdr.ClientData()
            ) { EditAs = Xdr.EditAsValues.TwoCell };

            drawingsPart.WorksheetDrawing.AppendChild(twoCellAnchor);
            drawingsPart.WorksheetDrawing.Save();

            if (worksheetPart.Worksheet.Elements<S.Drawing>().FirstOrDefault() == null)
            {
                worksheetPart.Worksheet.AppendChild(new S.Drawing { Id = worksheetPart.GetIdOfPart(drawingsPart) });
            }
        }

        private static C.ChartSpace CreateChartSpace(Chart chart)
        {
            var chartSpace = new C.ChartSpace();
            chartSpace.AddNamespaceDeclaration("c", "http://schemas.openxmlformats.org/drawingml/2006/chart");
            chartSpace.AddNamespaceDeclaration("a", "http://schemas.openxmlformats.org/drawingml/2006/main");
            chartSpace.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");

            chartSpace.AppendChild(new C.Date1904 { Val = false });
            chartSpace.AppendChild(new C.EditingLanguage { Val = "en-US" });
            chartSpace.AppendChild(new C.RoundedCorners { Val = false });

            var chartElem = chartSpace.AppendChild(new C.Chart());

            if (!String.IsNullOrWhiteSpace(chart.Title))
            {
                var title = new C.Title();
                title.AppendChild(new C.ChartText(new C.RichText(
                    new A.BodyProperties(),
                    new A.ListStyle(),
                    new A.Paragraph(
                        new A.ParagraphProperties(new A.DefaultRunProperties()),
                        new A.Run(
                            new A.RunProperties { Language = "en-US" },
                            new A.Text(chart.Title)
                        )
                    )
                )));
                title.AppendChild(new C.Layout());
                title.AppendChild(new C.Overlay { Val = false });
                chartElem.AppendChild(title);
            }

            chartElem.AppendChild(new C.AutoTitleDeleted { Val = String.IsNullOrWhiteSpace(chart.Title) });

            var plotArea = chartElem.AppendChild(new C.PlotArea());
            plotArea.AppendChild(new C.Layout());

            uint axisId1 = 148921728;
            uint axisId2 = 153678080;

            switch (chart.Type)
            {
                case ChartType.Pie:
                    CreatePieChart(plotArea, chart);
                    break;
                case ChartType.Line:
                    CreateLineChart(plotArea, chart, axisId1, axisId2);
                    break;
                case ChartType.Bar:
                default:
                    CreateBarChart(plotArea, chart, axisId1, axisId2);
                    break;
            }

            if (chart.ShowLegend)
            {
                var legend = new C.Legend(
                    new C.LegendPosition { Val = C.LegendPositionValues.Right },
                    new C.Layout(),
                    new C.Overlay { Val = false }
                );
                chartElem.AppendChild(legend);
            }

            chartElem.AppendChild(new C.PlotVisibleOnly { Val = true });
            chartElem.AppendChild(new C.DisplayBlanksAs { Val = C.DisplayBlanksAsValues.Zero });
            chartElem.AppendChild(new C.ShowDataLabelsOverMaximum { Val = false });

            return chartSpace;
        }

        private static void CreateBarChart(C.PlotArea plotArea, Chart chart, uint axisId1, uint axisId2)
        {
            var barChart = plotArea.AppendChild(new C.BarChart(
                new C.BarDirection { Val = C.BarDirectionValues.Column },
                new C.BarGrouping { Val = C.BarGroupingValues.Clustered },
                new C.VaryColors { Val = false }
            ));

            for (int i = 0; i < chart.Series.Count; i++)
            {
                var series = chart.Series[i];
                var barSeries = barChart.AppendChild(new C.BarChartSeries(
                    new C.Index { Val = (UInt32)i },
                    new C.Order { Val = (UInt32)i },
                    CreateSeriesText(series.Header)
                ));

                if (series.Color != null)
                {
                    barSeries.AppendChild(CreateSeriesShapeProperties(series.Color.Value));
                }

                if (chart.Categories.Count > 0)
                {
                    barSeries.AppendChild(CreateCategoryAxisData(chart.Categories));
                }

                barSeries.AppendChild(CreateValues(series.Values));
            }

            barChart.AppendChild(new C.AxisId { Val = axisId1 });
            barChart.AppendChild(new C.AxisId { Val = axisId2 });

            plotArea.AppendChild(new C.CategoryAxis(
                new C.AxisId { Val = axisId1 },
                new C.Scaling(new C.Orientation { Val = C.OrientationValues.MinMax }),
                new C.Delete { Val = false },
                new C.AxisPosition { Val = C.AxisPositionValues.Bottom },
                new C.CrossingAxis { Val = axisId2 },
                new C.Crosses { Val = C.CrossesValues.AutoZero },
                new C.AutoLabeled { Val = true },
                new C.LabelAlignment { Val = C.LabelAlignmentValues.Center }
            ));

            plotArea.AppendChild(new C.ValueAxis(
                new C.AxisId { Val = axisId2 },
                new C.Scaling(new C.Orientation { Val = C.OrientationValues.MinMax }),
                new C.Delete { Val = false },
                new C.AxisPosition { Val = C.AxisPositionValues.Left },
                new C.CrossingAxis { Val = axisId1 },
                new C.Crosses { Val = C.CrossesValues.AutoZero }
            ));
        }

        private static void CreateLineChart(C.PlotArea plotArea, Chart chart, uint axisId1, uint axisId2)
        {
            var lineChart = plotArea.AppendChild(new C.LineChart(
                new C.Grouping { Val = C.GroupingValues.Standard },
                new C.VaryColors { Val = false }
            ));

            for (int i = 0; i < chart.Series.Count; i++)
            {
                var series = chart.Series[i];
                var lineSeries = lineChart.AppendChild(new C.LineChartSeries(
                    new C.Index { Val = (UInt32)i },
                    new C.Order { Val = (UInt32)i },
                    CreateSeriesText(series.Header)
                ));

                if (series.Color != null)
                {
                    var shapeProps = new C.ChartShapeProperties();
                    var outline = new A.Outline();
                    var solidFill = new A.SolidFill();
                    solidFill.AppendChild(new A.RgbColorModelHex { Val = series.Color.Value.ToHexString().TrimStart('#') });
                    outline.AppendChild(solidFill);
                    shapeProps.AppendChild(outline);
                    lineSeries.AppendChild(shapeProps);
                }

                if (chart.Categories.Count > 0)
                {
                    lineSeries.AppendChild(CreateCategoryAxisData(chart.Categories));
                }

                lineSeries.AppendChild(CreateValues(series.Values));
            }

            lineChart.AppendChild(new C.AxisId { Val = axisId1 });
            lineChart.AppendChild(new C.AxisId { Val = axisId2 });

            plotArea.AppendChild(new C.CategoryAxis(
                new C.AxisId { Val = axisId1 },
                new C.Scaling(new C.Orientation { Val = C.OrientationValues.MinMax }),
                new C.Delete { Val = false },
                new C.AxisPosition { Val = C.AxisPositionValues.Bottom },
                new C.CrossingAxis { Val = axisId2 },
                new C.Crosses { Val = C.CrossesValues.AutoZero },
                new C.AutoLabeled { Val = true },
                new C.LabelAlignment { Val = C.LabelAlignmentValues.Center }
            ));

            plotArea.AppendChild(new C.ValueAxis(
                new C.AxisId { Val = axisId2 },
                new C.Scaling(new C.Orientation { Val = C.OrientationValues.MinMax }),
                new C.Delete { Val = false },
                new C.AxisPosition { Val = C.AxisPositionValues.Left },
                new C.CrossingAxis { Val = axisId1 },
                new C.Crosses { Val = C.CrossesValues.AutoZero }
            ));
        }

        private static void CreatePieChart(C.PlotArea plotArea, Chart chart)
        {
            var pieChart = plotArea.AppendChild(new C.PieChart(
                new C.VaryColors { Val = true }
            ));

            for (int i = 0; i < chart.Series.Count; i++)
            {
                var series = chart.Series[i];
                var pieSeries = pieChart.AppendChild(new C.PieChartSeries(
                    new C.Index { Val = (UInt32)i },
                    new C.Order { Val = (UInt32)i },
                    CreateSeriesText(series.Header)
                ));

                if (chart.Categories.Count > 0)
                {
                    pieSeries.AppendChild(CreateCategoryAxisData(chart.Categories));
                }

                pieSeries.AppendChild(CreateValues(series.Values));
            }
        }

        private static C.SeriesText CreateSeriesText(String? header)
        {
            var seriesText = new C.SeriesText();
            if (!String.IsNullOrEmpty(header))
            {
                seriesText.AppendChild(new C.NumericValue(header));
            }
            return seriesText;
        }

        private static C.CategoryAxisData CreateCategoryAxisData(List<String> categories)
        {
            var catAxisData = new C.CategoryAxisData();
            var stringLiteral = new C.StringLiteral();
            stringLiteral.AppendChild(new C.PointCount { Val = (UInt32)categories.Count });
            for (int i = 0; i < categories.Count; i++)
            {
                var stringPoint = new C.StringPoint { Index = (UInt32)i };
                stringPoint.AppendChild(new C.NumericValue(categories[i]));
                stringLiteral.AppendChild(stringPoint);
            }
            catAxisData.AppendChild(stringLiteral);
            return catAxisData;
        }

        private static C.Values CreateValues(List<Double> values)
        {
            var vals = new C.Values();
            var numLiteral = new C.NumberLiteral();
            numLiteral.AppendChild(new C.FormatCode("General"));
            numLiteral.AppendChild(new C.PointCount { Val = (UInt32)values.Count });
            for (int i = 0; i < values.Count; i++)
            {
                var numPoint = new C.NumericPoint { Index = (UInt32)i };
                numPoint.AppendChild(new C.NumericValue(values[i].ToString(CultureInfo.InvariantCulture)));
                numLiteral.AppendChild(numPoint);
            }
            vals.AppendChild(numLiteral);
            return vals;
        }

        private static C.ChartShapeProperties CreateSeriesShapeProperties(ColorInfo color)
        {
            var shapeProps = new C.ChartShapeProperties();
            var solidFill = new A.SolidFill();
            solidFill.AppendChild(new A.RgbColorModelHex { Val = color.ToHexString().TrimStart('#') });
            shapeProps.AppendChild(solidFill);
            return shapeProps;
        }
    }
}
