using System;
using Omtv.Api.Primitives;

namespace Omtv.Api.Model
{
    public class Table
    {
        public String? Name { get; private set; }
        public Style? Style { get; private set; } = null!;
        public Int32 Index { get; private set; }
        public TableRow Row { get; }

        public Table()
        {
            Row = new TableRow();
        }

        internal void Set(String? name, Style? style)
        {
            Index++;
            Row.ResetIndex();
            Name = name;
            Style = style;
        }
    }
}