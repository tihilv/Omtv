using System;
using Omtv.Api.Primitives;

namespace Omtv.Api.Model
{
    public class TableRow
    {
        public Style? Style { get; private set; } = null!;
        public Int32 Index { get; private set; }
        public Boolean IsHeader { get; private set; }
        public Measure? Height { get; private set; }
        public TableCell Cell { get; }

        public TableRow()
        {
            Cell = new TableCell(this);
        }

        internal void Set(Measure? height, Boolean isHeader, Style? style)
        {
            Index++;
            Cell.ResetIndex();
            Style = style;
            IsHeader = isHeader;
            Height = height;
        }

        internal void ResetIndex()
        {
            Index = 0;
            Cell.ResetIndex();
        }
    }
}