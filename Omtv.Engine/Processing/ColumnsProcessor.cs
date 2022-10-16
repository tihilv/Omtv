using System;
using System.Data;
using System.Threading.Tasks;
using System.Xml;
using Omtv.Api.Processing;

namespace Omtv.Engine.Processing
{
    public class ColumnsProcessor : IPartProcessor
    {
        public String Name => "columns";

        private readonly IPartProcessor[] _processors = new[]
        {
            new ColumnProcessor()
        };
        
        public async ValueTask ProcessAsync(XmlReader reader, ProcessingContext context)
        {
            if (context.Document.Table.Columns.Count > 0)
                throw new ConstraintException("Second 'columns' tag is not allowed inside a single table.");
            
            if (context.Document.Table.Row.Index >0)
                throw new ConstraintException("'columns' tag should be located before all the rows.");
            
            await context.Flow.ProcessAsync(reader, context, _processors);
        }
    }
}