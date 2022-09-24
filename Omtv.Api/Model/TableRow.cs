using System;
using Omtv.Api.Primitives;

namespace Omtv.Api.Model
{
    public class TableRow
    {
        public Style? Style { get; private set; } = null!;
        public Int32 Index { get; private set; }
        
        public Measure? Height { get; private set; }
        public TableCell Cell { get; }

        public TableRow()
        {
            Cell = new TableCell();
        }

        internal void Set(Measure? height, Style? style)
        {
            Index++;
            Cell.ResetIndex();
            Style = style;
            Height = height;
        }

        internal void ResetIndex()
        {
            Index = 0;
            Cell.ResetIndex();
        }
    }
}