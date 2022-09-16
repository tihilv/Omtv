using System;
using System.Threading.Tasks;
using System.Xml;
using Omtv.Api.Processing;

namespace Omtv.Engine.Processing
{
    public class RowProcessor: IPartProcessor
    {
        public String Name => "row";

        private readonly IPartProcessor[] _processors = new[]
        {
            new CellProcessor()
        };
        
        public async Task ProcessAsync(XmlReader reader, ProcessingContext context)
        {
            var newStyle = StyleProcessor.GetStyle(reader);
            var style = StyleProcessor.CombineStyle(context, newStyle);
            context.Document.CurrentTable.CurrentRow.Set(style);

            context.Output.RowStart(context.Document);
            await context.Flow.ProcessAsync(reader, context, _processors);
            CellProcessor.ProcessSpannedCells(context);
            context.Output.RowEnd(context.Document);
        }
    }
}