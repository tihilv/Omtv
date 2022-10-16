using System;
using System.Threading.Tasks;
using System.Xml;

namespace Omtv.Api.Processing
{
    public interface IPartProcessor
    {
        public String Name { get; }
        public ValueTask ProcessAsync(XmlReader reader, ProcessingContext context);
    }
}