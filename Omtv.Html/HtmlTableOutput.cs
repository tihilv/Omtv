﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Omtv.Api.Model;
using Omtv.Api.Primitives;
using Omtv.Api.Processing;
using Omtv.Api.Utils;

namespace Omtv.Html
{
    public class HtmlTableOutput: ITableOutput
    {
        private readonly StreamWriter _writer;
        
        public HtmlTableOutput(Stream outputStream)
        {
            _writer = new StreamWriter(outputStream, Encoding.UTF8, 1024, true);
        }

        public async ValueTask StartAsync(Document document)
        {
            await _writer.WriteLineAsync("<!DOCTYPE html>");
            await _writer.WriteLineAsync("<html>");
            await _writer.WriteLineAsync("<head>");
            await _writer.WriteLineAsync("<meta charset=\"UTF-8\">");

            if (!String.IsNullOrEmpty(document.Header.DocumentName))
                await _writer.WriteLineAsync($"<title>{document.Header.DocumentName}</title>");

            await _writer.WriteLineAsync("<style>");
            await _writer.WriteLineAsync($"@page {{ size: {Express(document.Header.PageWidth)} {Express(document.Header.PageHeight)};{Express(document.Header.Margin)}}}");
            await _writer.WriteLineAsync("table { border-spacing: 0; }");
            await _writer.WriteLineAsync("tr { border-spacing: 0; }");
            await _writer.WriteLineAsync("td { border-spacing: 0; }");
            foreach (var style in document.Styles.Values)
                await _writer.WriteLineAsync($".{style.Name} {{ {ExpressStyle(style, true)} }}");
            await _writer.WriteLineAsync("</style>");

            await _writer.WriteLineAsync("</head>");
            await _writer.WriteLineAsync("<body>");
        }

        private ColumnWidthProcessor _columnProcessor;
        private Boolean[] _processedColumns;
        public async ValueTask TableStartAsync(Document document)
        {
            if (!String.IsNullOrEmpty(document.Table.Name))
                await _writer.WriteLineAsync($"<h2>{document.Table.Name}</h2>");
            await _writer.WriteLineAsync($"<table{ExpressStyle(document.Table.Style, additional: new Dictionary<String, String?>(){["width"] = Express(document.Header.PageWidth)})}>");
            
            _processedColumns = new Boolean[document.Table.Columns.Count];
            
            _columnProcessor = new ColumnWidthProcessor(document, measure => ExpressWidth(measure, document.Header.ContentWidth));
        }

        public async ValueTask RowStartAsync(Document document)
        {
            await _writer.WriteLineAsync($"<tr{ExpressStyle(document.Table.Row.Style, additional: new Dictionary<String, String?>(){["height"] = Express(document.Table.Row.Height)})}>");
        }

        public async ValueTask CellAsync(Document document)
        {
            var tag = (document.Table.Row.Cell.IsHeader) ? "th" : "td";
            if (!document.Table.Row.Cell.Spanned)
            {
                var content = HttpUtility.HtmlEncode(document.Table.Row.Cell.Content!);
                if (String.IsNullOrEmpty(content))
                    content = "&nbsp;";
                content = content.Replace(Environment.NewLine, "<br>");

                String colSpan = String.Empty;
                if (document.Table.Row.Cell.ColSpan > 1)
                    colSpan = $" colspan=\"{document.Table.Row.Cell.ColSpan}\"";

                String rowSpan = String.Empty;
                if (document.Table.Row.Cell.RowSpan > 1)
                    rowSpan = $" rowspan=\"{document.Table.Row.Cell.RowSpan}\"";


                Dictionary<String, String?>? additionalProperties = null;
                var index = document.Table.Row.Cell.Index-1;
                if (_processedColumns.Length > index && !_processedColumns[index] && String.IsNullOrEmpty(colSpan))
                {
                    additionalProperties = new Dictionary<String, String?>() { ["width"] = Express(new Measure(_columnProcessor.ColumnWidths[index], document.Header.ContentWidth.Unit)) };
                    _processedColumns[index] = true;
                }
                await _writer.WriteLineAsync($"<{tag}{ExpressStyle(document.Table.Row.Cell.Style, additional: additionalProperties )}{colSpan}{rowSpan}>{content}</{tag}>");
            }
        }

        public async ValueTask RowEndAsync(Document document)
        {
            await _writer.WriteLineAsync("</tr>");
        }

        public async ValueTask TableEndAsync(Document document)
        {
            await _writer.WriteLineAsync($"</table>");
        }

        public async ValueTask EndAsync(Document document)
        {
            await _writer.WriteLineAsync("</body>");
            await _writer.WriteLineAsync("</html>");
            await _writer.FlushAsync();
        }

        private String ExpressStyle(Style? style, Boolean inHeader = false, Dictionary<String, String?>? additional = null)
        {
            var sb = new StringBuilder();

            if (style != null)
            {
                if (style.BackColor != null)
                    AppendStyle("background-color", style.BackColor.Value.ToHexString());

                if (style.ForeColor != null)
                    AppendStyle("color", style.ForeColor.Value.ToHexString());

                if (style.Border != null)
                {
                    var allBorder = style.Border[Side.All];
                    if (allBorder != null)
                        AppendStyle("border", ExpressBorder(allBorder.Value, style));
                    else
                    {
                        AppendStyle("border-left", ExpressBorder(style.Border[Side.Left], style));
                        AppendStyle("border-right", ExpressBorder(style.Border[Side.Right], style));
                        AppendStyle("border-top", ExpressBorder(style.Border[Side.Top], style));
                        AppendStyle("border-bottom", ExpressBorder(style.Border[Side.Bottom], style));
                    }
                }

                if (style.Font != null)
                {
                    AppendStyle("font-family", style.Font.Value.Family);
                    AppendStyle("font-size", style.Font.Value.Size.ToString());
                }

                AppendStyle("text-align", Express(style.HorizontalAlignment));
                AppendStyle("vertical-align", Express(style.VerticalAlignment, true));
            }

            if (additional != null)
                foreach (var pair in additional)
                    AppendStyle(pair.Key, pair.Value);
            
            if (inHeader)
                return sb.ToString();

            String parents = String.Empty;
            if (style?.Parents != null)
                parents = $" class=\"{String.Join(' ', style.Parents)}\"";

            if (sb.Length > 0)
                return $" style=\"{sb}\"{parents}";

            return parents;

            void AppendStyle(String property, String? value)
            {
                if (value != null)
                {
                    if (sb.Length > 0)
                        sb.Append("; ");

                    sb.Append($"{property}: {value}");
                }
            }

            String? ExpressBorder(BorderSide? borderSide, Style style)
            {
                if (borderSide == null)
                    return null;

                var thickness = borderSide.Value.Thickness ?? Measure.Null;
                var color = borderSide.Value.Color ?? style.ForeColor;
                return $"{Express(thickness)} {color?.ToHexString()} solid";
            }
        }

        private Double ExpressWidth(Measure measure, Measure pageWidth)
        {
            switch (measure.Unit)
            {
                case Unit.Pixel:
                    return measure.Value;
                case Unit.Em:
                    return measure.Value;
                case Unit.Percent:
                    return ExpressWidth(new Measure(pageWidth.Value * measure.Value / 100.0, pageWidth.Unit), pageWidth);
                case Unit.Mm:
                    return measure.Value;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private String? Express(Measure? measure)
        {
            if (measure == null)
                return null;
            
            String unit;
            switch (measure.Value.Unit)
            {
                case Unit.Pixel:
                    unit = "px";
                    break;
                case Unit.Em:
                    unit = "em";
                    break;
                case Unit.Percent:
                    unit = "%";
                    break;
                case Unit.Mm:
                    unit = "mm";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return $"{measure.Value.Value}{unit}";
        }

        private String? Express(Alignment? alignment, Boolean vertical = false)
        {
            if (alignment == null)
                return null;

            if (!vertical)
                switch (alignment)
                {
                    case Alignment.Before:
                        return "left";
                    case Alignment.Center:
                        return "center";
                    case Alignment.After:
                        return "right";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
                }
            switch (alignment)
            {
                case Alignment.Before:
                    return "top";
                case Alignment.Center:
                    return "center";
                case Alignment.After:
                    return "bottom";
                default:
                    throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
            }
        }
        
        private String Express(Margin? margin)
        {
            if (margin == null)
                return String.Empty;

            var sb = new StringBuilder();

            if (margin[Side.Left] != null)
                sb.Append($" margin-left: {Express(margin[Side.Left])};");
            if (margin[Side.Right] != null)
                sb.Append($" margin-right: {Express(margin[Side.Right])};");
            if (margin[Side.Top] != null)
                sb.Append($" margin-top: {Express(margin[Side.Top])};");
            if (margin[Side.Bottom] != null)
                sb.Append($" margin-bottom: {Express(margin[Side.Bottom])};");
            
            return sb.ToString();
        }
    }
}