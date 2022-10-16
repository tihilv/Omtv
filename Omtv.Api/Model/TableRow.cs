using System;
using Omtv.Api.Primitives;
using Omtv.Api.Processing;

namespace Omtv.Api.Model
{
    public class TableRow
    {
        private readonly Document _document;
        private Style? _combinedStyle;

        public Style? Style { get; private set; } = null!;
        public Int32 Index { get; private set; }
        public Boolean IsHeader { get; private set; }
        public Measure? Height { get; private set; }
        public TableCell Cell { get; }

        public TableRow(Document document)
        {
            _document = document;
            Cell = new TableCell(document);
        }

        internal void Set(Measure? height, Boolean isHeader, Style? style)
        {
            Index++;
            Cell.ResetIndex();
            _combinedStyle = null;
            Style = style;
            IsHeader = isHeader;
            Height = height;
        }

        internal void ResetIndex()
        {
            Index = 0;
            Cell.ResetIndex();
        }
        
        public Style GetCombinedStyle()
        {
            if (_combinedStyle == null)
            {
                _combinedStyle = StyleCombiner.CombineStyles(_document.Table.GetCombinedStyle(), Style, _document);
            }

            return _combinedStyle;
        }
    }
}