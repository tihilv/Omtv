using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Omtv.Api.Primitives;

[assembly: InternalsVisibleTo("Omtv.Engine")]

namespace Omtv.Api.Model
{
    public class Document
    {
        internal SpanStore SpanStore { get; private set; }
        
        public Header Header { get; }
        public Dictionary<String, Style> Styles { get; }
        public Table Table { get; }

        public Document()
        {
            Header = new Header();
            Styles = new Dictionary<String, Style>();
            Table = new Table(this);
            SpanStore = new SpanStore(Table);
        }
    }
}