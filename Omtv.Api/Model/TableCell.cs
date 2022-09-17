using System;
using Omtv.Api.Primitives;

namespace Omtv.Api.Model
{
    public class TableCell
    {
        public Style Style { get; private set; } = null!;
        public String? Content { get; private set; }
        public Byte RowSpan { get; private set; }
        public Byte ColSpan { get; private set; }
        public Int32 Index { get; private set; }
        public Boolean Spanned { get; private set; }

        internal void Set(Style style, String? content, Byte rowSpan, Byte colSpan)
        {
            Index++;
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
            Set(Style, null, 0, 0);
            Spanned = true;
        }
    }
}