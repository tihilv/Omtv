using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Omtv.Api.Primitives;

[assembly: InternalsVisibleTo("Omtv.Engine")]

namespace Omtv.Api.Model
{
    public class Document
    {
        private Table _currentTable;
        
        internal SpanProcessor SpanProcessor { get; private set; }
        
        public Header Header { get; set; }
        public Dictionary<String, Style> Styles { get; set; }
        public Int32 TableIndex { get; private set; }
        public Table CurrentTable
        {
            get => _currentTable;
            set
            {
                _currentTable = value;
                TableIndex++;
            }
        }

        public Document()
        {
            Header = new Header();
            Styles = new Dictionary<String, Style>();
            _currentTable = new Table();
            TableIndex = 0;
            SpanProcessor = new SpanProcessor(_currentTable);
        }
    }
}