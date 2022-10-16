using System;
using System.Collections.Generic;
using Omtv.Api.Primitives;
using Omtv.Api.Processing;

namespace Omtv.Api.Model
{
    public class Table
    {
        private readonly Document _document;
        private Style? _combinedStyle;
        private readonly List<TableColumn> _columns;
        
        public String? Name { get; private set; }
        public Style? Style { get; private set; } = null!;
        public Int32 Index { get; private set; }
        public TableRow Row { get; }

        public IReadOnlyList<TableColumn> Columns => _columns;

        public Table(Document document)
        {
            _document = document;
            Row = new TableRow(document);
            _columns = new List<TableColumn>();
        }

        internal void Set(String? name, Style? style)
        {
            Index++;
            Row.ResetIndex();
            _columns.Clear();
            Name = name;
            _combinedStyle = null;
            Style = style;
        }

        public Style GetCombinedStyle()
        {
            if (_combinedStyle == null)
            {
                _combinedStyle = StyleCombiner.CombineStyles(null, Style, _document);
            }

            return _combinedStyle;
        }

        internal void AddColumn(TableColumn column)
        {
            _columns.Add(column);
        }
    }
}