using System;
using Omtv.Api.Primitives;

namespace Omtv.Api.Model
{
    public class Header
    {
        public String? DocumentName { get; set; }
        public Measure PageWidth { get; private set; }
        public Measure PageHeight { get; private set; }
        public Margin? Margin { get; private set; }

        private const String UnitsExceptionText = "Page size and margin units should be the same";
        public Measure ContentWidth
        {
            get
            {
                var pageSize = PageWidth;
                if (Margin == null)
                    return pageSize;

                var left = Margin[Side.Left];
                var right = Margin[Side.Right];

                if (left == null && right == null)
                    return pageSize;

                var value = pageSize.Value;
                if (left != null)
                {
                    if (left.Value.Unit != pageSize.Unit)
                        throw new ArgumentException(UnitsExceptionText);

                    value -= left.Value.Value;
                }

                if (right != null)
                {
                    if (right.Value.Unit != pageSize.Unit)
                        throw new ArgumentException(UnitsExceptionText);

                    value -= right.Value.Value;
                }

                return new Measure(value, pageSize.Unit);
            }
        }

        public Measure ContentHeight
        {
            get
            {
                var pageSize = PageHeight;
                if (Margin == null)
                    return pageSize;

                var top = Margin[Side.Top];
                var bottom = Margin[Side.Bottom];

                if (top == null && bottom == null)
                    return pageSize;

                var value = pageSize.Value;
                if (top != null)
                {
                    if (top.Value.Unit != pageSize.Unit)
                        throw new ArgumentException(UnitsExceptionText);

                    value -= top.Value.Value;
                }

                if (bottom != null)
                {
                    if (bottom.Value.Unit != pageSize.Unit)
                        throw new ArgumentException(UnitsExceptionText);

                    value -= bottom.Value.Value;
                }

                return new Measure(value, pageSize.Unit);
            }
        }

        
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
        
        internal void SetMargin(Margin? value)
        {
            Margin = value;
        }
    }
}