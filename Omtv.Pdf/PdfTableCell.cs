using System;
using PdfSharpCore.Drawing;

namespace Omtv.Pdf
{
    internal class PdfTableCell
    {
        public XUnit Left { get; }
        public XUnit Width { get; }
        public Int32 RowStart { get; }
        public Int32 RowEnd { get; }
        public XFont Font { get; }

        public String? Content { get; }
        public XBrush BackBrush { get; }
        public XBrush ForeBrush { get; }
        public XStringFormat StringFormat { get; }
        public PdfBorders Borders { get; }

        public PdfTableCell(XUnit left, XUnit width, Int32 rowStart, Int32 rowEnd, XFont font, String? content, XBrush backBrush, XBrush foreBrush, XStringFormat stringFormat, PdfBorders borders)
        {
            RowStart = rowStart;
            RowEnd = rowEnd;
            Font = font;
            Width = width;
            Content = content;
            BackBrush = backBrush;
            ForeBrush = foreBrush;
            StringFormat = stringFormat;
            Borders = borders;
            Left = left;
        }
    }

    internal struct PdfBorders
    {
        public readonly XPen? Left;
        public readonly XPen? Top;
        public readonly XPen? Right;
        public readonly XPen? Bottom;

        public PdfBorders(XPen? left, XPen? right, XPen? top, XPen? bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }
}