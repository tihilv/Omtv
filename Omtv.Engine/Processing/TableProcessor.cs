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
        
        public async Task ProcessAsync(XmlReader reader, ProcessingContext context)
        {
            var tableName = reader.GetAttribute(TableName);
            var newStyle = StyleProcessor.GetStyle(reader);
            var style = StyleProcessor.CombineStyle(context, newStyle);
            context.Document.CurrentTable.Set(tableName, style);

            context.Output.TableStart(context.Document);
            await context.Flow.ProcessAsync(reader, context, _processors);
            context.Output.TableEnd(context.Document);
        }
    }
}