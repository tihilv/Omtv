using System;
using System.Threading.Tasks;
using System.Xml;
using Omtv.Api.Model;
using Omtv.Api.Primitives;
using Omtv.Api.Processing;

namespace Omtv.Engine.Processing
{
    public class ColumnProcessor : IPartProcessor
    {
        private const String WidthName = "width";
        
        public String Name => "column";

        public async ValueTask ProcessAsync(XmlReader reader, ProcessingContext context)
        {
            var width = Measure.Parse(reader.GetAttribute(WidthName));
            context.Document.Table.AddColumn(new TableColumn(width));
        }
    }
}