using System;
using Omtv.Api.Primitives;

namespace Omtv.Api.Model
{
    public class Header
    {
        public String? DocumentName { get; set; }
        public Measure PageWidth { get; private set; }
        public Measure PageHeight { get; private set; }

        internal void SetPageHeight(Measure value)
        {
            PageHeight = value;
        }
        
        internal void SetPageWidth(Measure value)
        {
            PageWidth = value;
        }

        internal void SetDocumentName(String value)
        {
            DocumentName = value;
        }
    }
}