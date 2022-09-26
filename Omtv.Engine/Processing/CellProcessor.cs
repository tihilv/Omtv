using System;
using System.Threading.Tasks;
using System.Xml;
using Omtv.Api.Primitives;
using Omtv.Api.Processing;

namespace Omtv.Engine.Processing
{
    public class CellProcessor: IPartProcessor
    {
        private const String WidthName = "width";
        private const String HeaderName = "header";
        private const String RowSpanName = "rowSpan";
        private const String ColSpanName = "colSpan";

        public String Name => "cell";

        public async Task ProcessAsync(XmlReader reader, ProcessingContext context)
        {
            var style = StyleProcessor.GetStyle(reader);

            var rowSpanStr = reader.GetAttribute(RowSpanName);
            var colSpanStr = reader.GetAttribute(ColSpanName);
            var header = reader.GetAttribute(HeaderName) != null;

            if (!Byte.TryParse(rowSpanStr, out var rowSpan))
                rowSpan = 1;
            if (!Byte.TryParse(colSpanStr, out var colSpan))
                colSpan = 1;

            var width = Measure.Parse(reader.GetAttribute(WidthName));
            
            await reader.ReadAsync();
            var value = PrepareValue(reader.Value);
            
            await ProcessSpannedCellsAsync(context);

            context.Document.Table.Row.Cell.Set(value, width, rowSpan, colSpan, header, style);
            context.Document.SpanStore.Register(rowSpan, colSpan);
            await context.Output.CellAsync(context.Document);
        }

        private String? PrepareValue(String value)
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