using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using Omtv.Api.Model;
using Omtv.Api.Primitives;

namespace Omtv.Engine.Processing
{
    internal static class ChartProcessor
    {
        private const String TypeAttribute = "type";
        private const String HeaderAttribute = "header";
        private const String NameAttribute = "name";
        private const String ColorAttribute = "color";
        private const String ValuesAttribute = "values";
        private const String CategoriesName = "categories";
        private const String TitleName = "title";
        private const String LegendName = "legend";
        private const String ShowLegendName = "showlegend";
        private const String ChartElementName = "chart";
        private const String LabelsElementName = "labels";
        private const String CategoryElementName = "category";
        private const String LabelElementName = "label";
        private const String SeriesElementName = "series";
        private const String ValueElementName = "value";
        private const String ValElementName = "val";

        public static async ValueTask<Chart> ProcessAsync(XmlReader reader)
        {
            var typeStr = reader.GetAttribute(TypeAttribute);
            var chartType = ChartType.Bar;
            if (!String.IsNullOrEmpty(typeStr))
            {
                if (Enum.TryParse<ChartType>(typeStr, true, out var parsedType))
                    chartType = parsedType;
            }

            var showLegend = ParseBool(reader.GetAttribute(ShowLegendName) ?? reader.GetAttribute(LegendName));
            var title = reader.GetAttribute(TitleName) ?? reader.GetAttribute(HeaderAttribute);
            var categoriesAttr = reader.GetAttribute(CategoriesName);

            var chart = new Chart(chartType, showLegend)
            {
                Title = title
            };

            if (!String.IsNullOrWhiteSpace(categoriesAttr))
            {
                AddCategories(chart.Categories, categoriesAttr);
            }

            if (reader.IsEmptyElement)
            {
                return chart;
            }

            var chartDepth = reader.Depth;

            while (await reader.ReadAsync())
            {
                if (reader.Depth <= chartDepth && reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals(ChartElementName, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    var elementName = reader.Name.ToLowerInvariant();
                    switch (elementName)
                    {
                        case CategoriesName:
                        case LabelsElementName:
                            await ProcessCategoriesElementAsync(reader, chart);
                            break;
                        case CategoryElementName:
                        case LabelElementName:
                            var categoryText = await reader.ReadElementContentAsStringAsync();
                            if (!String.IsNullOrWhiteSpace(categoryText))
                                chart.Categories.Add(categoryText.Trim());
                            break;
                        case TitleName:
                            chart.Title = (await reader.ReadElementContentAsStringAsync())?.Trim();
                            break;
                        case LegendName:
                        case ShowLegendName:
                            var legText = await reader.ReadElementContentAsStringAsync();
                            chart.ShowLegend = ParseBool(legText);
                            break;
                        case SeriesElementName:
                            await ProcessSeriesElementAsync(reader, chart);
                            break;
                    }
                }
            }

            return chart;
        }

        private static async ValueTask ProcessCategoriesElementAsync(XmlReader reader, Chart chart)
        {
            if (reader.IsEmptyElement)
                return;

            var depth = reader.Depth;
            var text = "";

            while (await reader.ReadAsync())
            {
                if (reader.Depth <= depth && reader.NodeType == XmlNodeType.EndElement)
                    break;

                if (reader.NodeType == XmlNodeType.Element && (reader.Name.Equals(CategoryElementName, StringComparison.OrdinalIgnoreCase) || reader.Name.Equals(LabelElementName, StringComparison.OrdinalIgnoreCase)))
                {
                    var cat = await reader.ReadElementContentAsStringAsync();
                    if (!String.IsNullOrWhiteSpace(cat))
                        chart.Categories.Add(cat.Trim());
                }
                else if (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA)
                {
                    text += reader.Value;
                }
            }

            if (!String.IsNullOrWhiteSpace(text))
            {
                AddCategories(chart.Categories, text);
            }
        }

        private static async ValueTask ProcessSeriesElementAsync(XmlReader reader, Chart chart)
        {
            var header = reader.GetAttribute(HeaderAttribute) ?? reader.GetAttribute(NameAttribute) ?? reader.GetAttribute(TitleName);
            var colorStr = reader.GetAttribute(ColorAttribute);
            var valuesAttr = reader.GetAttribute(ValuesAttribute);

            var color = ColorInfo.Parse(colorStr);
            var series = new ChartSeries(header, color);

            if (!String.IsNullOrWhiteSpace(valuesAttr))
            {
                AddValues(series.Values, valuesAttr);
            }

            if (!reader.IsEmptyElement)
            {
                var depth = reader.Depth;
                var text = "";

                while (await reader.ReadAsync())
                {
                    if (reader.Depth <= depth && reader.NodeType == XmlNodeType.EndElement)
                        break;

                    if (reader.NodeType == XmlNodeType.Element && (reader.Name.Equals(ValueElementName, StringComparison.OrdinalIgnoreCase) || reader.Name.Equals(ValElementName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var valStr = await reader.ReadElementContentAsStringAsync();
                        if (Double.TryParse(valStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                            series.Values.Add(d);
                    }
                    else if (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA)
                    {
                        text += reader.Value;
                    }
                }

                if (!String.IsNullOrWhiteSpace(text))
                {
                    AddValues(series.Values, text);
                }
            }

            chart.Series.Add(series);
        }

        private static void AddCategories(List<String> categories, String text)
        {
            var parts = text.Split(new[] { ',', ';', '|', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (!String.IsNullOrEmpty(trimmed))
                    categories.Add(trimmed);
            }
        }

        private static void AddValues(List<Double> values, String text)
        {
            var parts = text.Split(new[] { ',', ';', ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (Double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
                    values.Add(val);
            }
        }

        private static Boolean ParseBool(String? value)
        {
            if (String.IsNullOrEmpty(value))
                return false;

            value = value.Trim();
            return value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("1") ||
                   value.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }
    }
}
