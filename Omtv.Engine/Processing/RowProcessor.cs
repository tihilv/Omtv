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
        private const String HeaderName = "header";
        
        public String Name => "row";

        private readonly IPartProcessor[] _processors = new[]
        {
            new CellProcessor()
        };
        
        public async ValueTask ProcessAsync(XmlReader reader, ProcessingContext context)
        {
            if (context.Document.Table.Row.Index == 0)
                await context.Output.TableStartAsync(context.Document);
            
            var style = StyleProcessor.GetStyle(reader);
            var height = Measure.Parse(reader.GetAttribute(HeightName));
            var header = reader.GetAttribute(HeaderName) != null;
            
            context.Document.Table.Row.Set(height, header, style);

            await context.Output.RowStartAsync(context.Document);
            await context.Flow.ProcessAsync(reader, context, _processors);
            await CellProcessor.ProcessSpannedCellsAsync(context);
            await context.Output.RowEndAsync(context.Document);
        }
    }
}