using System;
using System.Threading.Tasks;
using System.Xml;
using Omtv.Api.Primitives;
using Omtv.Api.Processing;

namespace Omtv.Engine.Processing
{
    public class CellProcessor: IPartProcessor
    {
        public String Name => "cell";
        private const String WidthName = "width";
        public String RowSpanName => "rowSpan";
        public String ColSpanName => "colSpan";

        public async Task ProcessAsync(XmlReader reader, ProcessingContext context)
        {
            var style = StyleProcessor.GetStyle(reader);

            var rowSpanStr = reader.GetAttribute(RowSpanName);
            var colSpanStr = reader.GetAttribute(ColSpanName);

            if (!Byte.TryParse(rowSpanStr, out var rowSpan))
                rowSpan = 1;
            if (!Byte.TryParse(colSpanStr, out var colSpan))
                colSpan = 1;

            var width = Measure.Parse(reader.GetAttribute(WidthName));
            
            await reader.ReadAsync();
            var value = reader.Value;
            
            await ProcessSpannedCellsAsync(context);

            context.Document.Table.Row.Cell.Set(value, width, rowSpan, colSpan, style);
            context.Document.SpanStore.Register(rowSpan, colSpan);
            await context.Output.CellAsync(context.Document);
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