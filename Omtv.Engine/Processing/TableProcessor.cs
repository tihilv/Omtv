using System;
using System.Threading.Tasks;
using System.Xml;
using Omtv.Api.Processing;

namespace Omtv.Engine.Processing
{
    public class TableProcessor: IPartProcessor
    {
        private const String TableName = "name";
        
        public String Name => "table";

        private readonly IPartProcessor[] _processors = new[]
        {
            new RowProcessor()
        };

        private Boolean _initialized;
        public async Task ProcessAsync(XmlReader reader, ProcessingContext context)
        {
            var tableName = reader.GetAttribute(TableName);
            var style = StyleProcessor.GetStyle(reader);
            context.Document.Table.Set(tableName, style);

            if (!_initialized)
            {
                _initialized = true;
                await context.Output.StartAsync(context.Document);
            }

            await context.Output.TableStartAsync(context.Document);
            await context.Flow.ProcessAsync(reader, context, _processors);
            await context.Output.TableEndAsync(context.Document);
        }
    }
}