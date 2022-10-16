using System;
using Omtv.Api.Primitives;
using Omtv.Api.Processing;

namespace Omtv.Api.Model
{
    public class TableCell
    {
        private readonly Document _document;
        private Style? _combinedStyle;
        
        private Boolean _isHeader;

        public Style? Style { get; private set; } = null!;
        public String? Content { get; private set; }
        public Byte RowSpan { get; private set; }
        public Byte ColSpan { get; private set; }
        public Int32 Index { get; private set; }
        public Boolean Spanned { get; private set; }
        public Boolean IsHeader => _isHeader || _document.Table.Row.IsHeader;

        public TableCell(Document document)
        {
            _document = document;
        }
        
        internal void Set(String? content, Byte rowSpan, Byte colSpan, Boolean isHeader, Style? style)
        {
            Index++;
            _combinedStyle = null;
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
            Set(null, 0, 0, false, Style);
            Spanned = true;
        }
        
        public Style GetCombinedStyle()
        {
            if (_combinedStyle == null)
            {
                _combinedStyle = StyleCombiner.CombineStyles(_document.Table.Row.GetCombinedStyle(), Style, _document);
            }

            return _combinedStyle;
        }
    }
}