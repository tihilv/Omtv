using Omtv.Api.Model;

namespace Omtv.Api.Processing
{
    public class ProcessingContext
    {
        public Document Document { get; }
        public IProcessingFlow Flow { get; }
        public ITableOutput Output { get; }

        public ProcessingContext(Document document, IProcessingFlow flow, ITableOutput output)
        {
            Document = document;
            Flow = flow;
            Output = output;
        }
    }
}