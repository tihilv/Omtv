using System;
using Omtv.Api.Primitives;

namespace Omtv.Api.Model
{
    public class TableCell
    {
        public Style? Style { get; private set; } = null!;
        public String? Content { get; private set; }
        public Byte RowSpan { get; private set; }
        public Byte ColSpan { get; private set; }
        public Int32 Index { get; private set; }
        public Boolean Spanned { get; private set; }
        public Measure? Width { get; private set; }

        internal void Set(String? content, Measure? width, Byte rowSpan, Byte colSpan, Style? style)
        {
            Index++;
            Width = width;
            Style = style;
            Content = content;
            RowSpan = rowSpan;
            ColSpan = colSpan;
            Spanned = false;
        }
        
        internal void ResetIndex()
        {
            Index = 0;
        }

        internal void SetSpanned()
        {
            Set(null, Measure.Null, 0, 0, Style);
            Spanned = true;
        }
    }
}