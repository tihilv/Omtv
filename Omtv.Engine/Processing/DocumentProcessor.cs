using System;
using System.Threading.Tasks;
using System.Xml;
using Omtv.Api.Processing;

namespace Omtv.Engine.Processing
{
    public class DocumentProcessor: IPartProcessor
    {
        public String Name => "document";

        private readonly IPartProcessor[] _processors = new IPartProcessor[]
        {
            new HeaderProcessor(),
            new TableProcessor()
        };
        
        public async Task ProcessAsync(XmlReader reader, ProcessingContext context)
        {
            await context.Flow.ProcessAsync(reader, context, _processors);
        }
    }
}