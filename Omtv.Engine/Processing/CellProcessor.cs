using System;
using System.Threading.Tasks;
using System.Xml;
using Omtv.Api.Model;
using Omtv.Api.Processing;

namespace Omtv.Engine.Processing
{
    public class CellProcessor: IPartProcessor
    {
        private const String HeaderName = "header";
        private const String RowSpanName = "rowSpan";
        private const String ColSpanName = "colSpan";

        public String Name => "cell";

        public async ValueTask ProcessAsync(XmlReader reader, ProcessingContext context)
        {
            var style = StyleProcessor.GetStyle(reader);

            var rowSpanStr = reader.GetAttribute(RowSpanName);
            var colSpanStr = reader.GetAttribute(ColSpanName);
            var header = reader.GetAttribute(HeaderName) != null;

            if (!Byte.TryParse(rowSpanStr, out var rowSpan))
                rowSpan = 1;
            if (!Byte.TryParse(colSpanStr, out var colSpan))
                colSpan = 1;

            String? value = null;
            Chart? chart = null;

            if (!reader.IsEmptyElement)
            {
                var cellDepth = reader.Depth;
                while (await reader.ReadAsync())
                {
                    if (reader.Depth <= cellDepth && reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals("cell", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("chart", StringComparison.OrdinalIgnoreCase))
                    {
                        chart = await ChartProcessor.ProcessAsync(reader);
                    }
                    else if (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA || reader.NodeType == XmlNodeType.SignificantWhitespace)
                    {
                        value = (value ?? "") + reader.Value;
                    }
                }
            }

            value = PrepareValue(value);
            
            await ProcessSpannedCellsAsync(context);

            context.Document.Table.Row.Cell.Set(value, rowSpan, colSpan, header, style, chart);
            context.Document.SpanStore.Register(rowSpan, colSpan);
            await context.Output.CellAsync(context.Document);
        }

        private String? PrepareValue(String? value)
        {
            if (String.IsNullOrEmpty(value))
                return value;

            return value.Replace("\\n", Environment.NewLine);
        }

        internal static async ValueTask ProcessSpannedCellsAsync(ProcessingContext context)
        {
            while (context.Document.SpanStore.IsSpanned())
            {
                context.Document.Table.Row.Cell.SetSpanned();
                await context.Output.CellAsync(context.Document);
            }
        }
    }
}