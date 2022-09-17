using System.Threading.Tasks;
using Omtv.Api.Model;

namespace Omtv.Api.Processing
{
    public interface ITableOutput
    {
        ValueTask TableStartAsync(Document document);
        ValueTask RowStartAsync(Document document);
        ValueTask CellAsync(Document document);
        ValueTask RowEndAsync(Document document);
        ValueTask TableEndAsync(Document document);
        ValueTask DoneAsync(Document document);
    }
}