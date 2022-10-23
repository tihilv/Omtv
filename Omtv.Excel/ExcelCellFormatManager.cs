using System;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using Omtv.Api.Primitives;
using Alignment = Omtv.Api.Primitives.Alignment;

namespace Omtv.Excel
{
    internal class ExcelCellFormatManager
    {
        private readonly ExcelStylesheetManager _stylesheetManager;
        private readonly CellFormats _cellFormats;

        public CellFormats CellFormats => _cellFormats;

        internal ExcelCellFormatManager(ExcelStylesheetManager stylesheetManager)
        {
            _stylesheetManager = stylesheetManager;
            _cellFormats = new CellFormats() { Count = 1 };
            _cellFormats.Append(new CellFormat() { NumberFormatId = 0U, FontId = 0U, FillId = 0U, BorderId = 0U, FormatId = 0U });
        }

        internal UInt32 GetCellPropertiesIndex(Cell cell, Style style, UInt32 borderId, Boolean wrapText, out CellFormat cellFormat)
        {
            cellFormat = GetCellFormat(cell);

            ProcessAlignment(cellFormat, style, wrapText);
            cellFormat.FontId = _stylesheetManager.FontManager.GetFont(style);
            cellFormat.FillId = _stylesheetManager.FillManager.GetFill(style);
            cellFormat.BorderId = borderId;

            return InsertCellFormat(cellFormat);
        }

        private static void ProcessAlignment(CellFormat cellFormat, Style style, Boolean wrapText)
        {
            var hor = GetHorizontalAlignment(style);
            var ver = GetVerticalAlignment(style);

            if (hor != null || ver != null || wrapText)
            {
                var alignment = new DocumentFormat.OpenXml.Spreadsheet.Alignment();

                if (hor != null)
                    alignment.Horizontal = hor;

                if (ver != null)
                    alignment.Vertical = ver;

                if (wrapText)
                    alignment.WrapText = true;
                
                cellFormat.Append(alignment);
                cellFormat.ApplyAlignment = true;
            }
        }

        private CellFormat GetCellFormat(Cell cell)
        {
            if (cell.StyleIndex != null)
                return (CellFormat)_stylesheetManager.Stylesheet.Elements<CellFormats>().First().Elements<CellFormat>().ElementAt((Int32)cell.StyleIndex.Value).CloneNode(true);

            return new CellFormat();
        }

        public UInt32 InsertCellFormat(CellFormat cellFormat)
        {
            CellFormats cellFormats = _stylesheetManager.Stylesheet.Elements<CellFormats>().First();
            cellFormats.Append(cellFormat);
            return (UInt32)cellFormats.Count++;
        }

        private static VerticalAlignmentValues? GetVerticalAlignment(Style style)
        {
            switch (style.VerticalAlignment)
            {
                case Alignment.Before:
                    return VerticalAlignmentValues.Top;
                case Alignment.Center:
                    return VerticalAlignmentValues.Center;
                case Alignment.After:
                    return VerticalAlignmentValues.Bottom;
                case null:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static HorizontalAlignmentValues? GetHorizontalAlignment(Style style)
        {
            switch (style.HorizontalAlignment)
            {
                case Alignment.Before:
                    return HorizontalAlignmentValues.Left;
                case Alignment.Center:
                    return HorizontalAlignmentValues.Center;
                case Alignment.After:
                    return HorizontalAlignmentValues.Right;
                case null:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        
    }
}