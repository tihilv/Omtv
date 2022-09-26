using System;
using Omtv.Api.Primitives;

namespace Omtv.Api.Model
{
    public class TableCell
    {
        private readonly TableRow _row;
        
        private Boolean _isHeader;

        public Style? Style { get; private set; } = null!;
        public String? Content { get; private set; }
        public Byte RowSpan { get; private set; }
        public Byte ColSpan { get; private set; }
        public Int32 Index { get; private set; }
        public Boolean Spanned { get; private set; }
        public Measure? Width { get; private set; }

        public Boolean IsHeader => _isHeader || _row.IsHeader;

        public TableCell(TableRow row)
        {
            _row = row;
        }
        
        internal void Set(String? content, Measure? width, Byte rowSpan, Byte colSpan, Boolean isHeader, Style? style)
        {
            Index++;
            Width = width;
            Style = style;
            Content = content;
            RowSpan = rowSpan;
            ColSpan = colSpan;
            _isHeader = isHeader;
            Spanned = false;
        }
        
        internal void ResetIndex()
        {
            Index = 0;
        }

        internal void SetSpanned()
        {
            Set(null, Measure.Null, 0, 0, false, Style);
            Spanned = true;
        }
    }
}