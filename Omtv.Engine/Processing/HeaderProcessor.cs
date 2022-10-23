using System;
using System.Threading.Tasks;
using System.Xml;
using Omtv.Api.Primitives;
using Omtv.Api.Processing;

namespace Omtv.Engine.Processing
{
    public class HeaderProcessor: IPartProcessor
    {
        private const String DocumentName = "name";
        private const String WidthName = "width";
        private const String HeightName = "height";
       
        public String Name => "header";

        private readonly IPartProcessor[] _processors = new[]
        {
            new StyleProcessor()
        };
        
        public async ValueTask ProcessAsync(XmlReader reader, ProcessingContext context)
        {
            var documentName = reader.GetAttribute(DocumentName);
            var width = reader.GetAttribute(WidthName);
            var height = reader.GetAttribute(HeightName);
            var margin = StyleProcessor.GetMargin(reader);
            
            context.Document.Header.SetDocumentName(documentName);
            context.Document.Header.SetPageHeight(Measure.ParseExact(height));
            context.Document.Header.SetPageWidth(Measure.ParseExact(width));
            
            if (margin != null)
                context.Document.Header.SetMargin(margin);

            await context.Flow.ProcessAsync(reader, context, _processors);
        }
    }
}