using System;
using System.Threading.Tasks;
using System.Xml;
using Omtv.Api.Primitives;
using Omtv.Api.Processing;

namespace Omtv.Engine.Processing
{
    public class RowProcessor: IPartProcessor
    {
        private const String HeightName = "height";
        
        public String Name => "row";

        private readonly IPartProcessor[] _processors = new[]
        {
            new CellProcessor()
        };
        
        public async Task ProcessAsync(XmlReader reader, ProcessingContext context)
        {
            var style = StyleProcessor.GetStyle(reader);
            var height = Measure.Parse(reader.GetAttribute(HeightName));
            
            context.Document.Table.Row.Set(height, style);

            await context.Output.RowStartAsync(context.Document);
            await context.Flow.ProcessAsync(reader, context, _processors);
            await CellProcessor.ProcessSpannedCellsAsync(context);
            await context.Output.RowEndAsync(context.Document);
        }
    }
}