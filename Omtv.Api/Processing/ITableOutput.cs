using Omtv.Api.Model;

namespace Omtv.Api.Processing
{
    public interface ITableOutput
    {
        void TableStart(Document document);
        void RowStart(Document document);
        void Cell(Document document);
        void RowEnd(Document document);
        void TableEnd(Document document);
    }
}