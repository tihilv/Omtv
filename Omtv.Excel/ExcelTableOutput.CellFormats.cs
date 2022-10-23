using System;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using Omtv.Api.Primitives;
using Alignment = Omtv.Api.Primitives.Alignment;

namespace Omtv.Excel
{
    public partial class ExcelTableOutput
    {
        private CellFormats PrepareDefaultCellFormats()
        {
            var cellFormats = new CellFormats() { Count = 1 };
            cellFormats.Append(new CellFormat() { NumberFormatId = 0U, FontId = 0U, FillId = 0U, BorderId = 0U, FormatId = 0U });
            return cellFormats;
        }

        private UInt32 GetCellPropertiesIndex(Cell cell, Style style, UInt32 borderId, Boolean wrapText, out CellFormat cellFormat)
        {
            cellFormat = GetCellFormat(cell);

            ProcessAlignment(cellFormat, style, wrapText);
            cellFormat.FontId = GetFont(style);
            cellFormat.FillId = GetFill(style);
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
                return _workbookStylesPart.Stylesheet.Elements<CellFormats>().First().Elements<CellFormat>().ElementAt((Int32)cell.StyleIndex.Value).CloneNode(true) as CellFormat;

            return new CellFormat();
        }

        public UInt32 InsertCellFormat(CellFormat cellFormat)
        {
            CellFormats cellFormats = _workbookStylesPart.Stylesheet.Elements<CellFormats>().First();
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

        private Double GetExcelHeight(Measure value, Measure pageSize)
        {
            switch (value.Unit)
            {
                case Unit.Pixel:
                    return value.Value / 4.0 * 3;
                case Unit.Em:
                    return value.Value;
                case Unit.Percent:
                    return GetExcelHeight(new Measure(pageSize.Value * value.Value / 100.0, pageSize.Unit), pageSize);
                case Unit.Mm:
                    return value.Value / 0.176378 / 4.0 * 3;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Double GetExcelWidthInInch(Measure value, Measure pageSize)
        {
            switch (value.Unit)
            {
                case Unit.Pixel:
                    return (value.Value * 0.0909 - 0.63)*1.03;
                case Unit.Em:
                    return value.Value;
                case Unit.Percent:
                    return GetExcelHeight(new Measure(pageSize.Value * value.Value / 100.0, pageSize.Unit), pageSize);
                case Unit.Mm:
                    return (value.Value-1.0)/2.0 + 0.54296875;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}