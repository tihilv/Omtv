using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Omtv.Api.Model;
using Omtv.Api.Processing;
using Omtv.Engine.Processing;

namespace Omtv.Engine
{
    public class TableVisualizer: IProcessingFlow
    {
        private readonly IPartProcessor[] _partProcessors;

        private TableVisualizer()
        {
            _partProcessors = new IPartProcessor[]
            {
                new DocumentProcessor()
            };
        }
        
        public static async Task TransformAsync(Stream inputStream, ITableOutput output)
        {
            var visualizer = new TableVisualizer();
            await visualizer.ProcessAsync(inputStream, output);
        }

        public static async Task TransformAsync(String input, ITableOutput output)
        {
            using (var inputStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(inputStream, Encoding.Default, 10240, true))
                    await writer.WriteLineAsync(input);

                inputStream.Position = 0;

                await TransformAsync(inputStream, output);
            }
        }

        private async Task ProcessAsync(Stream inputStream, ITableOutput output)
        {
            var xmlSettings = new XmlReaderSettings() { IgnoreComments = true, Async = true};
            var context = new ProcessingContext(new Document(), this, output);
            using (var reader = XmlReader.Create(inputStream, xmlSettings))
            {
                await ProcessAsync(reader, context, _partProcessors);
                await output.EndAsync(context.Document);
            }
        }

        public async Task ProcessAsync(XmlReader reader, ProcessingContext context, IPartProcessor[] processors)
        {
            string elementName;
            int depth = -1;
            while ((depth == -1 || depth == reader.Depth) && await reader.ReadAsync())
            {
                if (depth == -1)
                    depth = reader.Depth;
                
                if (reader.IsStartElement())
                {
                    elementName = reader.Name;

                    var processor = processors.FirstOrDefault(p => p.Name.Equals(elementName, StringComparison.InvariantCultureIgnoreCase));
                    if (processor == null)
                        throw new ArgumentException($"Unknown element '{reader.Name}'.");

                    await processor.ProcessAsync(reader, context);
                    if (reader.IsStartElement() || reader.Name != elementName)
                        await reader.ReadAsync(); // close tag
                }
                else
                    return;
            }
        }
    }
}