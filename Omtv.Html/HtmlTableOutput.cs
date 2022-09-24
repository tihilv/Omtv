using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Omtv.Api.Model;
using Omtv.Api.Primitives;
using Omtv.Api.Processing;

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
            await _writer.WriteLineAsync("table { border-spacing: 0; }");
            await _writer.WriteLineAsync("tr { border-spacing: 0; }");
            await _writer.WriteLineAsync("td { border-spacing: 0; }");
            foreach (var style in document.Styles.Values)
                await _writer.WriteLineAsync($".{style.Name} {{ {ExpressStyle(style, true)} }}");
            await _writer.WriteLineAsync("</style>");

            await _writer.WriteLineAsync("</head>");
            await _writer.WriteLineAsync("<body>");
        }

        public async ValueTask TableStartAsync(Document document)
        {
            if (!String.IsNullOrEmpty(document.Table.Name))
                await _writer.WriteLineAsync($"<h2>{document.Table.Name}</h2>");
            await _writer.WriteLineAsync($"<table{ExpressStyle(document.Table.Style, additional: new Dictionary<String, String?>(){["width"] = Express(document.Header.PageWidth)})}>");
        }

        public async ValueTask RowStartAsync(Document document)
        {
            await _writer.WriteLineAsync($"<tr{ExpressStyle(document.Table.Row.Style, additional: new Dictionary<String, String?>(){["height"] = Express(document.Table.Row.Height)})}>");
        }

        public async ValueTask CellAsync(Document document)
        {
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

                await _writer.WriteLineAsync($"<td{ExpressStyle(document.Table.Row.Cell.Style, additional: new Dictionary<String, String?>(){["width"] = Express(document.Table.Row.Cell.Width)})}{colSpan}{rowSpan}>{content}</td>");
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
    }
}