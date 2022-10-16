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

        private readonly IPartProcessor[] _processors = new IPartProcessor[]
        {
            new ColumnsProcessor(),
            new RowProcessor()
        };

        private Boolean _initialized;
        public async ValueTask ProcessAsync(XmlReader reader, ProcessingContext context)
        {
            var tableName = reader.GetAttribute(TableName);
            var style = StyleProcessor.GetStyle(reader);
            context.Document.Table.Set(tableName, style);

            if (!_initialized)
            {
                _initialized = true;
                await context.Output.StartAsync(context.Document);
            }

            await context.Flow.ProcessAsync(reader, context, _processors);
            if (context.Document.Table.Row.Index == 0)
                await context.Output.TableStartAsync(context.Document);
            await context.Output.TableEndAsync(context.Document);
        }
    }
}