using System;
using System.Threading.Tasks;
using System.Xml;
using Omtv.Api.Processing;

namespace Omtv.Engine.Processing
{
    public class CellProcessor: IPartProcessor
    {
        public String Name => "cell";
        public String RowSpanName => "rowSpan";
        public String ColSpanName => "colSpan";

        public async Task ProcessAsync(XmlReader reader, ProcessingContext context)
        {
            var newStyle = StyleProcessor.GetStyle(reader);
            var style = StyleProcessor.CombineStyle(context, newStyle);

            var rowSpanStr = reader.GetAttribute(RowSpanName);
            var colSpanStr = reader.GetAttribute(ColSpanName);

            if (!Byte.TryParse(rowSpanStr, out var rowSpan))
                rowSpan = 1;
            if (!Byte.TryParse(colSpanStr, out var colSpan))
                colSpan = 1;

            await reader.ReadAsync();
            var value = reader.Value;
            
            ProcessSpannedCells(context);

            context.Document.CurrentTable.CurrentRow.CurrentCell.Set(style, value, rowSpan, colSpan);
            context.Document.SpanProcessor.Register(rowSpan, colSpan);
            context.Output.Cell(context.Document);
        }

        internal static void ProcessSpannedCells(ProcessingContext context)
        {
            while (context.Document.SpanProcessor.IsSpanned())
            {
                context.Document.CurrentTable.CurrentRow.CurrentCell.SetSpanned();
                context.Output.Cell(context.Document);
            }
        }
    }
}