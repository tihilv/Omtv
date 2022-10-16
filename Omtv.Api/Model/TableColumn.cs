using Omtv.Api.Primitives;

namespace Omtv.Api.Model
{
    public class TableColumn
    {
        public Measure? Width { get; }

        public TableColumn(Measure? width)
        {
            Width = width;
        }
    }
}