using System;
using Omtv.Api.Primitives;

namespace Omtv.Api.Model
{
    public class TableRow
    {
        public Style Style { get; private set; } = null!;
        public Int32 Index { get; private set; }
        public TableCell CurrentCell { get; }

        public TableRow()
        {
            CurrentCell = new TableCell();
        }

        internal void Set(Style style)
        {
            Index++;
            CurrentCell.ResetIndex();
            Style = style;
        }

        internal void ResetIndex()
        {
            Index = 0;
            CurrentCell.ResetIndex();
        }
    }
}