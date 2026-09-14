using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Omtv.Api.Model;
using PdfSharpCore.Drawing;

namespace Omtv.Pdf
{
    internal static class PdfChartRenderer
    {
        private static readonly XColor[] DefaultColors = new[]
        {
            XColor.FromArgb(78, 121, 167),
            XColor.FromArgb(242, 142, 44),
            XColor.FromArgb(225, 87, 89),
            XColor.FromArgb(118, 183, 178),
            XColor.FromArgb(89, 161, 79),
            XColor.FromArgb(237, 201, 73),
            XColor.FromArgb(175, 122, 161),
            XColor.FromArgb(255, 157, 167),
            XColor.FromArgb(156, 117, 95),
            XColor.FromArgb(186, 176, 171)
        };

        public static void DrawChart(XGraphics gfx, Chart chart, XRect rect, XFont baseFont)
        {
            if (rect.Width <= 10 || rect.Height <= 10)
                return;

            var state = gfx.Save();
            gfx.IntersectClip(rect);

            var padding = 6.0;
            var width = rect.Width - (padding * 2);
            var height = rect.Height - (padding * 2);
            var left = rect.Left + padding;
            var top = rect.Top + padding;

            var topMargin = 5.0;
            var bottomMargin = chart.ShowLegend ? 28.0 : 18.0;
            var leftMargin = 28.0;
            var rightMargin = 10.0;

            if (!String.IsNullOrWhiteSpace(chart.Title))
            {
                var titleFont = new XFont(baseFont.Name, Math.Min(12, Math.Max(8, baseFont.Size * 0.9)), XFontStyle.Bold);
                var titleRect = new XRect(left, top, width, 16);
                gfx.DrawString(chart.Title, titleFont, XBrushes.Black, titleRect, XStringFormats.TopCenter);
                topMargin += 18.0;
            }

            switch (chart.Type)
            {
                case ChartType.Pie:
                    DrawPieChart(gfx, chart, left, top, width, height, topMargin, bottomMargin, leftMargin, rightMargin, baseFont);
                    break;
                case ChartType.Line:
                    DrawLineChart(gfx, chart, left, top, width, height, topMargin, bottomMargin, leftMargin, rightMargin, baseFont);
                    break;
                case ChartType.Bar:
                default:
                    DrawBarChart(gfx, chart, left, top, width, height, topMargin, bottomMargin, leftMargin, rightMargin, baseFont);
                    break;
            }

            if (chart.ShowLegend)
            {
                DrawLegend(gfx, chart, left, top, width, height, baseFont);
            }

            gfx.Restore(state);
        }

        private static XColor GetSeriesColor(ChartSeries series, Int32 index)
        {
            if (series.Color != null)
            {
                var c = series.Color.Value;
                return XColor.FromArgb(c.A, c.R, c.G, c.B);
            }
            return DefaultColors[index % DefaultColors.Length];
        }

        private static XColor GetSliceColor(Chart chart, Int32 sliceIndex)
        {
            if (chart.Series.Count > sliceIndex && chart.Series[sliceIndex].Color != null)
            {
                var c = chart.Series[sliceIndex].Color!.Value;
                return XColor.FromArgb(c.A, c.R, c.G, c.B);
            }
            if (chart.Series.Count > 0 && sliceIndex == 0 && chart.Series[0].Color != null)
            {
                var c = chart.Series[0].Color!.Value;
                return XColor.FromArgb(c.A, c.R, c.G, c.B);
            }
            return DefaultColors[sliceIndex % DefaultColors.Length];
        }

        private static void DrawBarChart(XGraphics gfx, Chart chart, Double left, Double top, Double width, Double height, Double topMargin, Double bottomMargin, Double leftMargin, Double rightMargin, XFont baseFont)
        {
            var plotLeft = left + leftMargin;
            var plotTop = top + topMargin;
            var plotWidth = width - leftMargin - rightMargin;
            var plotHeight = height - topMargin - bottomMargin;
            if (plotWidth <= 0 || plotHeight <= 0) return;

            var seriesList = chart.Series;
            var numSeries = Math.Max(1, seriesList.Count);
            var numCategories = chart.Categories.Count;
            if (numCategories == 0)
            {
                numCategories = seriesList.Select(s => s.Values.Count).DefaultIfEmpty(0).Max();
            }
            if (numCategories == 0) numCategories = 1;

            var allValues = seriesList.SelectMany(s => s.Values).ToList();
            var maxVal = allValues.Count > 0 ? Math.Max(0, allValues.Max()) : 10;
            if (maxVal == 0) maxVal = 10;
            var minVal = allValues.Count > 0 ? Math.Min(0, allValues.Min()) : 0;
            var range = maxVal - minVal;
            if (range == 0) range = 1;

            var labelFont = new XFont(baseFont.Name, Math.Min(8, Math.Max(6, baseFont.Size * 0.7)), XFontStyle.Regular);
            var gridPen = new XPen(XColor.FromArgb(230, 230, 230), 0.5);
            var axisPen = new XPen(XColor.FromArgb(120, 120, 120), 0.75);

            var steps = 4;
            for (var i = 0; i <= steps; i++)
            {
                var val = minVal + (range * i / steps);
                var y = plotTop + plotHeight - ((val - minVal) / range * plotHeight);
                gfx.DrawLine(gridPen, plotLeft, y, plotLeft + plotWidth, y);

                var label = val.ToString("0.##", CultureInfo.InvariantCulture);
                gfx.DrawString(label, labelFont, XBrushes.Gray, new XRect(left, y - 4, leftMargin - 3, 8), XStringFormats.TopRight);
            }

            var zeroY = plotTop + plotHeight - ((0 - minVal) / range * plotHeight);
            gfx.DrawLine(axisPen, plotLeft, zeroY, plotLeft + plotWidth, zeroY);

            var categoryWidth = plotWidth / numCategories;
            var barGroupWidth = categoryWidth * 0.8;
            var barWidth = barGroupWidth / numSeries;

            for (var c = 0; c < numCategories; c++)
            {
                var catLeft = plotLeft + (c * categoryWidth) + (categoryWidth * 0.1);

                for (var s = 0; s < seriesList.Count; s++)
                {
                    var series = seriesList[s];
                    if (c < series.Values.Count)
                    {
                        var val = series.Values[c];
                        var barHeight = Math.Abs(val) / range * plotHeight;
                        var barX = catLeft + (s * barWidth);
                        var barY = val >= 0 ? zeroY - barHeight : zeroY;
                        var color = GetSeriesColor(series, s);

                        var brush = new XSolidBrush(color);
                        gfx.DrawRectangle(brush, barX, barY, barWidth, barHeight);
                    }
                }

                var catLabel = c < chart.Categories.Count ? chart.Categories[c] : $"{(c + 1)}";
                var catRect = new XRect(plotLeft + (c * categoryWidth), plotTop + plotHeight + 3, categoryWidth, 12);
                gfx.DrawString(catLabel, labelFont, XBrushes.DarkGray, catRect, XStringFormats.TopCenter);
            }
        }

        private static void DrawLineChart(XGraphics gfx, Chart chart, Double left, Double top, Double width, Double height, Double topMargin, Double bottomMargin, Double leftMargin, Double rightMargin, XFont baseFont)
        {
            var plotLeft = left + leftMargin;
            var plotTop = top + topMargin;
            var plotWidth = width - leftMargin - rightMargin;
            var plotHeight = height - topMargin - bottomMargin;
            if (plotWidth <= 0 || plotHeight <= 0) return;

            var seriesList = chart.Series;
            var numCategories = chart.Categories.Count;
            if (numCategories == 0)
            {
                numCategories = seriesList.Select(s => s.Values.Count).DefaultIfEmpty(0).Max();
            }
            if (numCategories == 0) numCategories = 1;

            var allValues = seriesList.SelectMany(s => s.Values).ToList();
            var maxVal = allValues.Count > 0 ? Math.Max(0, allValues.Max()) : 10;
            if (maxVal == 0) maxVal = 10;
            var minVal = allValues.Count > 0 ? Math.Min(0, allValues.Min()) : 0;
            var range = maxVal - minVal;
            if (range == 0) range = 1;

            var labelFont = new XFont(baseFont.Name, Math.Min(8, Math.Max(6, baseFont.Size * 0.7)), XFontStyle.Regular);
            var gridPen = new XPen(XColor.FromArgb(230, 230, 230), 0.5);
            var axisPen = new XPen(XColor.FromArgb(120, 120, 120), 0.75);

            var steps = 4;
            for (var i = 0; i <= steps; i++)
            {
                var val = minVal + (range * i / steps);
                var y = plotTop + plotHeight - ((val - minVal) / range * plotHeight);
                gfx.DrawLine(gridPen, plotLeft, y, plotLeft + plotWidth, y);

                var label = val.ToString("0.##", CultureInfo.InvariantCulture);
                gfx.DrawString(label, labelFont, XBrushes.Gray, new XRect(left, y - 4, leftMargin - 3, 8), XStringFormats.TopRight);
            }

            var zeroY = plotTop + plotHeight - ((0 - minVal) / range * plotHeight);
            gfx.DrawLine(axisPen, plotLeft, zeroY, plotLeft + plotWidth, zeroY);

            // Category labels
            for (var c = 0; c < numCategories; c++)
            {
                var x = numCategories == 1 ? plotLeft + (plotWidth / 2) : plotLeft + (c * (plotWidth / (numCategories - 1)));
                var catLabel = c < chart.Categories.Count ? chart.Categories[c] : $"{(c + 1)}";
                var catRect = new XRect(x - 20, plotTop + plotHeight + 3, 40, 12);
                gfx.DrawString(catLabel, labelFont, XBrushes.DarkGray, catRect, XStringFormats.TopCenter);
            }

            // Lines and Points
            for (var s = 0; s < seriesList.Count; s++)
            {
                var series = seriesList[s];
                var color = GetSeriesColor(series, s);
                var pen = new XPen(color, 1.5);
                var brush = new XSolidBrush(color);

                XPoint? prevPoint = null;

                for (var c = 0; c < series.Values.Count; c++)
                {
                    var x = numCategories == 1 ? plotLeft + (plotWidth / 2) : plotLeft + (c * (plotWidth / (numCategories - 1)));
                    var val = series.Values[c];
                    var y = plotTop + plotHeight - ((val - minVal) / range * plotHeight);
                    var point = new XPoint(x, y);

                    if (prevPoint != null)
                    {
                        gfx.DrawLine(pen, prevPoint.Value, point);
                    }
                    prevPoint = point;

                    gfx.DrawEllipse(brush, x - 2.5, y - 2.5, 5, 5);
                }
            }
        }

        private static void DrawPieChart(XGraphics gfx, Chart chart, Double left, Double top, Double width, Double height, Double topMargin, Double bottomMargin, Double leftMargin, Double rightMargin, XFont baseFont)
        {
            var plotLeft = left + leftMargin;
            var plotTop = top + topMargin;
            var plotWidth = width - leftMargin - rightMargin;
            var plotHeight = height - topMargin - bottomMargin;
            if (plotWidth <= 0 || plotHeight <= 0) return;

            var cx = plotLeft + (plotWidth / 2.0);
            var cy = plotTop + (plotHeight / 2.0);
            var radius = Math.Min(plotWidth, plotHeight) / 2.2;

            var sliceValues = new List<Double>();
            if (chart.Series.Count == 1)
            {
                sliceValues.AddRange(chart.Series[0].Values);
            }
            else if (chart.Series.Count > 1)
            {
                foreach (var s in chart.Series)
                    sliceValues.Add(s.Values.Count > 0 ? s.Values[0] : 0);
            }

            var total = sliceValues.Where(v => v > 0).Sum();
            if (total <= 0)
            {
                gfx.DrawEllipse(XBrushes.LightGray, cx - radius, cy - radius, radius * 2, radius * 2);
                return;
            }

            Double currentAngle = -90.0;
            var whitePen = new XPen(XColors.White, 1);

            for (var i = 0; i < sliceValues.Count; i++)
            {
                var val = sliceValues[i];
                if (val <= 0) continue;

                var sweepAngle = (val / total) * 360.0;
                var color = GetSliceColor(chart, i);
                var brush = new XSolidBrush(color);

                if (Math.Abs(sweepAngle - 360.0) < 0.001)
                {
                    gfx.DrawEllipse(brush, cx - radius, cy - radius, radius * 2, radius * 2);
                }
                else
                {
                    gfx.DrawPie(brush, cx - radius, cy - radius, radius * 2, radius * 2, currentAngle, sweepAngle);
                    gfx.DrawPie(whitePen, cx - radius, cy - radius, radius * 2, radius * 2, currentAngle, sweepAngle);
                }

                currentAngle += sweepAngle;
            }
        }

        private static void DrawLegend(XGraphics gfx, Chart chart, Double left, Double top, Double width, Double height, XFont baseFont)
        {
            var items = new List<(String Label, XColor Color)>();

            if (chart.Type == ChartType.Pie)
            {
                if (chart.Categories.Count > 0)
                {
                    for (var i = 0; i < chart.Categories.Count; i++)
                        items.Add((chart.Categories[i], GetSliceColor(chart, i)));
                }
                else
                {
                    for (var i = 0; i < chart.Series.Count; i++)
                    {
                        var header = chart.Series[i].Header ?? $"Slice {i + 1}";
                        items.Add((header, GetSliceColor(chart, i)));
                    }
                }
            }
            else
            {
                for (var i = 0; i < chart.Series.Count; i++)
                {
                    var header = chart.Series[i].Header ?? $"Series {i + 1}";
                    items.Add((header, GetSeriesColor(chart.Series[i], i)));
                }
            }

            if (items.Count == 0) return;

            var legendFont = new XFont(baseFont.Name, Math.Min(8, Math.Max(6, baseFont.Size * 0.7)), XFontStyle.Regular);
            var y = top + height - 12;
            var totalEstWidth = items.Sum(it => it.Label.Length * 5 + 18);
            var currX = Math.Max(left + 10, left + (width - totalEstWidth) / 2.0);

            foreach (var item in items)
            {
                var brush = new XSolidBrush(item.Color);
                gfx.DrawRectangle(brush, currX, y - 6, 8, 8);
                gfx.DrawString(item.Label, legendFont, XBrushes.DimGray, new XRect(currX + 11, y - 7, item.Label.Length * 6 + 10, 10), XStringFormats.TopLeft);
                currX += item.Label.Length * 5 + 18;
            }
        }
    }
}
