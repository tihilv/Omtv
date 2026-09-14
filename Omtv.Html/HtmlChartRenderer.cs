using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using Omtv.Api.Model;

namespace Omtv.Html
{
    internal static class HtmlChartRenderer
    {
        private static readonly String[] DefaultPalette = new[]
        {
            "#4e79a7", "#f28e2c", "#e15759", "#76b7b2", "#59a14f",
            "#edc949", "#af7aa1", "#ff9da7", "#9c755f", "#bab0ab"
        };

        public static String RenderSvg(Chart chart, Double width = 450, Double height = 280)
        {
            var sb = new StringBuilder();
            sb.Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {width} {height}\" width=\"100%\" height=\"100%\" style=\"font-family: sans-serif; display: block;\">");

            Double topMargin = 20;
            Double bottomMargin = chart.ShowLegend ? 50 : 35;
            Double leftMargin = 45;
            Double rightMargin = 20;

            if (!String.IsNullOrWhiteSpace(chart.Title))
            {
                topMargin += 20;
                sb.Append($"<text x=\"{width / 2}\" y=\"20\" text-anchor=\"middle\" font-size=\"14\" font-weight=\"bold\" fill=\"#333\">{HttpUtility.HtmlEncode(chart.Title)}</text>");
            }

            switch (chart.Type)
            {
                case ChartType.Pie:
                    RenderPieChart(sb, chart, width, height, topMargin, bottomMargin, leftMargin, rightMargin);
                    break;
                case ChartType.Line:
                    RenderLineChart(sb, chart, width, height, topMargin, bottomMargin, leftMargin, rightMargin);
                    break;
                case ChartType.Bar:
                default:
                    RenderBarChart(sb, chart, width, height, topMargin, bottomMargin, leftMargin, rightMargin);
                    break;
            }

            if (chart.ShowLegend)
            {
                RenderLegend(sb, chart, width, height);
            }

            sb.Append("</svg>");
            return sb.ToString();
        }

        private static String GetSeriesColor(ChartSeries series, Int32 index)
        {
            if (series.Color != null)
                return series.Color.Value.ToHexString();
            return DefaultPalette[index % DefaultPalette.Length];
        }

        private static String GetSliceColor(Chart chart, Int32 sliceIndex)
        {
            if (chart.Series.Count > sliceIndex && chart.Series[sliceIndex].Color != null)
                return chart.Series[sliceIndex].Color!.Value.ToHexString();
            if (chart.Series.Count > 0 && sliceIndex == 0 && chart.Series[0].Color != null)
                return chart.Series[0].Color!.Value.ToHexString();
            return DefaultPalette[sliceIndex % DefaultPalette.Length];
        }

        private static void RenderBarChart(StringBuilder sb, Chart chart, Double width, Double height, Double top, Double bottom, Double left, Double right)
        {
            var plotWidth = width - left - right;
            var plotHeight = height - top - bottom;
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

            // Grid lines & Y-axis labels
            var steps = 4;
            for (var i = 0; i <= steps; i++)
            {
                var val = minVal + (range * i / steps);
                var y = top + plotHeight - ((val - minVal) / range * plotHeight);
                sb.Append($"<line x1=\"{left}\" y1=\"{y.ToString(CultureInfo.InvariantCulture)}\" x2=\"{left + plotWidth}\" y2=\"{y.ToString(CultureInfo.InvariantCulture)}\" stroke=\"#e0e0e0\" stroke-width=\"1\"/>");
                sb.Append($"<text x=\"{(left - 6).ToString(CultureInfo.InvariantCulture)}\" y=\"{(y + 4).ToString(CultureInfo.InvariantCulture)}\" text-anchor=\"end\" font-size=\"10\" fill=\"#666\">{val.ToString("0.##", CultureInfo.InvariantCulture)}</text>");
            }

            // X axis line
            var zeroY = top + plotHeight - ((0 - minVal) / range * plotHeight);
            sb.Append($"<line x1=\"{left}\" y1=\"{zeroY.ToString(CultureInfo.InvariantCulture)}\" x2=\"{left + plotWidth}\" y2=\"{zeroY.ToString(CultureInfo.InvariantCulture)}\" stroke=\"#888\" stroke-width=\"1\"/>");

            // Bars
            var categoryWidth = plotWidth / numCategories;
            var barGroupWidth = categoryWidth * 0.8;
            var barWidth = barGroupWidth / numSeries;

            for (var c = 0; c < numCategories; c++)
            {
                var catLeft = left + (c * categoryWidth) + (categoryWidth * 0.1);

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

                        sb.Append($"<rect x=\"{barX.ToString(CultureInfo.InvariantCulture)}\" y=\"{barY.ToString(CultureInfo.InvariantCulture)}\" width=\"{barWidth.ToString(CultureInfo.InvariantCulture)}\" height=\"{barHeight.ToString(CultureInfo.InvariantCulture)}\" fill=\"{color}\" rx=\"1\"><title>{HttpUtility.HtmlEncode(series.Header ?? $"Series {s+1}")}: {val}</title></rect>");
                    }
                }

                // Category label
                var catLabel = c < chart.Categories.Count ? chart.Categories[c] : $"{(c + 1)}";
                var catCenterX = left + (c * categoryWidth) + (categoryWidth / 2.0);
                sb.Append($"<text x=\"{catCenterX.ToString(CultureInfo.InvariantCulture)}\" y=\"{(top + plotHeight + 15).ToString(CultureInfo.InvariantCulture)}\" text-anchor=\"middle\" font-size=\"10\" fill=\"#444\">{HttpUtility.HtmlEncode(catLabel)}</text>");
            }
        }

        private static void RenderLineChart(StringBuilder sb, Chart chart, Double width, Double height, Double top, Double bottom, Double left, Double right)
        {
            var plotWidth = width - left - right;
            var plotHeight = height - top - bottom;
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

            // Grid lines & Y-axis labels
            var steps = 4;
            for (var i = 0; i <= steps; i++)
            {
                var val = minVal + (range * i / steps);
                var y = top + plotHeight - ((val - minVal) / range * plotHeight);
                sb.Append($"<line x1=\"{left}\" y1=\"{y.ToString(CultureInfo.InvariantCulture)}\" x2=\"{left + plotWidth}\" y2=\"{y.ToString(CultureInfo.InvariantCulture)}\" stroke=\"#e0e0e0\" stroke-width=\"1\"/>");
                sb.Append($"<text x=\"{(left - 6).ToString(CultureInfo.InvariantCulture)}\" y=\"{(y + 4).ToString(CultureInfo.InvariantCulture)}\" text-anchor=\"end\" font-size=\"10\" fill=\"#666\">{val.ToString("0.##", CultureInfo.InvariantCulture)}</text>");
            }

            // X axis line
            var zeroY = top + plotHeight - ((0 - minVal) / range * plotHeight);
            sb.Append($"<line x1=\"{left}\" y1=\"{zeroY.ToString(CultureInfo.InvariantCulture)}\" x2=\"{left + plotWidth}\" y2=\"{zeroY.ToString(CultureInfo.InvariantCulture)}\" stroke=\"#888\" stroke-width=\"1\"/>");

            // Category labels
            for (var c = 0; c < numCategories; c++)
            {
                var x = numCategories == 1 ? left + (plotWidth / 2) : left + (c * (plotWidth / (numCategories - 1)));
                var catLabel = c < chart.Categories.Count ? chart.Categories[c] : $"{(c + 1)}";
                sb.Append($"<text x=\"{x.ToString(CultureInfo.InvariantCulture)}\" y=\"{(top + plotHeight + 15).ToString(CultureInfo.InvariantCulture)}\" text-anchor=\"middle\" font-size=\"10\" fill=\"#444\">{HttpUtility.HtmlEncode(catLabel)}</text>");
            }

            // Lines and Points
            for (var s = 0; s < seriesList.Count; s++)
            {
                var series = seriesList[s];
                var color = GetSeriesColor(series, s);
                var points = new List<String>();

                for (var c = 0; c < series.Values.Count; c++)
                {
                    var x = numCategories == 1 ? left + (plotWidth / 2) : left + (c * (plotWidth / (numCategories - 1)));
                    var val = series.Values[c];
                    var y = top + plotHeight - ((val - minVal) / range * plotHeight);
                    points.Add($"{x.ToString(CultureInfo.InvariantCulture)},{y.ToString(CultureInfo.InvariantCulture)}");
                }

                if (points.Count > 1)
                {
                    sb.Append($"<polyline points=\"{String.Join(" ", points)}\" fill=\"none\" stroke=\"{color}\" stroke-width=\"2.5\"/>");
                }

                for (var c = 0; c < series.Values.Count; c++)
                {
                    var x = numCategories == 1 ? left + (plotWidth / 2) : left + (c * (plotWidth / (numCategories - 1)));
                    var val = series.Values[c];
                    var y = top + plotHeight - ((val - minVal) / range * plotHeight);
                    sb.Append($"<circle cx=\"{x.ToString(CultureInfo.InvariantCulture)}\" cy=\"{y.ToString(CultureInfo.InvariantCulture)}\" r=\"4\" fill=\"{color}\" stroke=\"#fff\" stroke-width=\"1.5\"><title>{HttpUtility.HtmlEncode(series.Header ?? $"Series {s+1}")}: {val}</title></circle>");
                }
            }
        }

        private static void RenderPieChart(StringBuilder sb, Chart chart, Double width, Double height, Double top, Double bottom, Double left, Double right)
        {
            var plotWidth = width - left - right;
            var plotHeight = height - top - bottom;
            if (plotWidth <= 0 || plotHeight <= 0) return;

            var cx = left + (plotWidth / 2.0);
            var cy = top + (plotHeight / 2.0);
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
                sb.Append($"<circle cx=\"{cx.ToString(CultureInfo.InvariantCulture)}\" cy=\"{cy.ToString(CultureInfo.InvariantCulture)}\" r=\"{radius.ToString(CultureInfo.InvariantCulture)}\" fill=\"#ccc\"/>");
                return;
            }

            Double currentAngle = -90.0; // Start at top (12 o'clock)

            for (var i = 0; i < sliceValues.Count; i++)
            {
                var val = sliceValues[i];
                if (val <= 0) continue;

                var sweepAngle = (val / total) * 360.0;
                var color = GetSliceColor(chart, i);

                if (Math.Abs(sweepAngle - 360.0) < 0.001)
                {
                    sb.Append($"<circle cx=\"{cx.ToString(CultureInfo.InvariantCulture)}\" cy=\"{cy.ToString(CultureInfo.InvariantCulture)}\" r=\"{radius.ToString(CultureInfo.InvariantCulture)}\" fill=\"{color}\"/>");
                }
                else
                {
                    var startRad = currentAngle * Math.PI / 180.0;
                    var endRad = (currentAngle + sweepAngle) * Math.PI / 180.0;

                    var x1 = cx + (radius * Math.Cos(startRad));
                    var y1 = cy + (radius * Math.Sin(startRad));
                    var x2 = cx + (radius * Math.Cos(endRad));
                    var y2 = cy + (radius * Math.Sin(endRad));

                    var largeArcFlag = sweepAngle > 180.0 ? 1 : 0;

                    sb.Append($"<path d=\"M {cx.ToString(CultureInfo.InvariantCulture)} {cy.ToString(CultureInfo.InvariantCulture)} L {x1.ToString(CultureInfo.InvariantCulture)} {y1.ToString(CultureInfo.InvariantCulture)} A {radius.ToString(CultureInfo.InvariantCulture)} {radius.ToString(CultureInfo.InvariantCulture)} 0 {largeArcFlag} 1 {x2.ToString(CultureInfo.InvariantCulture)} {y2.ToString(CultureInfo.InvariantCulture)} Z\" fill=\"{color}\" stroke=\"#fff\" stroke-width=\"1.5\"/>");
                }

                currentAngle += sweepAngle;
            }
        }

        private static void RenderLegend(StringBuilder sb, Chart chart, Double width, Double height)
        {
            var items = new List<(String Label, String Color)>();

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

            var y = height - 20;
            var totalEstimatedWidth = items.Sum(it => it.Label.Length * 7 + 25);
            var startX = Math.Max(20.0, (width - totalEstimatedWidth) / 2.0);

            var currX = startX;
            foreach (var item in items)
            {
                sb.Append($"<rect x=\"{currX.ToString(CultureInfo.InvariantCulture)}\" y=\"{(y - 8).ToString(CultureInfo.InvariantCulture)}\" width=\"10\" height=\"10\" fill=\"{item.Color}\" rx=\"2\"/>");
                sb.Append($"<text x=\"{(currX + 14).ToString(CultureInfo.InvariantCulture)}\" y=\"{y.ToString(CultureInfo.InvariantCulture)}\" font-size=\"10\" fill=\"#555\">{HttpUtility.HtmlEncode(item.Label)}</text>");
                currX += item.Label.Length * 7 + 25;
            }
        }
    }
}
