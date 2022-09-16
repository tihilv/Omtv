using System.Threading.Tasks;
using System.Xml;

namespace Omtv.Api.Processing
{
    public interface IProcessingFlow
    {
        Task ProcessAsync(XmlReader reader, ProcessingContext context, IPartProcessor[] processors);
    }
}