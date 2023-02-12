using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Omtv.Api.Model;
using Omtv.Api.Primitives;
using Omtv.Api.Processing;
using Omtv.Api.Utils;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Pdf;

namespace Omtv.Pdf
{
    public class PdfTableOutput: ITableOutput
    {
        private static readonly FontInfo DefaultFont = new FontInfo("Times New Roman", new Measure(12, Unit.Em));
        
        private readonly Stream _stream;
        private readonly PdfDocument _pdfDocument;

        private readonly FontCache _fontCache;
        private readonly GraphicsCache _graphicsCache;
        
        private ColumnWidthProcessor _columnWidthProcessor;
        private List<List<PdfTableCell>> _currentRows;
        private List<PdfTableCell> _currentRowCells;
        private List<XUnit> _rowsHeight;

        private XGraphics _measureContext;
        private XUnit _currentLeft;
        
        static PdfTableOutput()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public PdfTableOutput(Stream stream)
        {
            _stream = stream;
            
            _pdfDocument = new PdfDocument();
            _fontCache = new FontCache();
            _graphicsCache = new GraphicsCache();
        }

        public ValueTask StartAsync(Document document)
        {
            _pdfDocument.Info.Title = document.Header.DocumentName;
            return new ValueTask();
        }

        public ValueTask TableStartAsync(Document document)
        {
            _currentRows = new List<List<PdfTableCell>>();
            _rowsHeight = new List<XUnit>();

            _columnWidthProcessor = new ColumnWidthProcessor(document, measure => measure.ToXUnit(document.Header.ContentWidth));
            _measureContext = GetMeasureContext(document);

            return new ValueTask();
        }

        private XGraphics GetMeasureContext(Document document)
        {
            var pageWidth = document.Header.ContentWidth.ToXUnit();
            var pageHeight = document.Header.ContentHeight.ToXUnit();
            return XGraphics.CreateMeasureContext(new XSize(pageWidth, pageHeight), pageWidth.Type, XPageDirection.Downwards);
        }

        public ValueTask RowStartAsync(Document document)
        {
            _currentRowCells = new List<PdfTableCell>();
            _currentRows.Add(_currentRowCells);
            SetRowHeight(document.Table.Row.Index, (document.Table.Row.Height ?? Measure.Null).ToXUnit(document.Header.ContentHeight));
            _currentLeft = XUnit.Zero;
            return new ValueTask();
        }

        private void SetRowHeight(Int32 index, XUnit value)
        {
            while (_rowsHeight.Count < index)
                _rowsHeight.Add(XUnit.Zero);

            if (_rowsHeight[index-1] < value)
                _rowsHeight[index-1] = value;
        }

        public ValueTask CellAsync(Document document)
        {
            var row = document.Table.Row;
            var cell = row.Cell;

            XUnit cellWidth;
            if (!cell.Spanned)
            {
                var style = cell.GetCombinedStyle();
                var fontInfo = style.Font ?? DefaultFont;

                XFont font = _fontCache.GetFont(fontInfo, document.Table.Row.IsHeader);

                var measure = _measureContext.MeasureString(cell.Content, font);
                var normalizedRowHeight = measure.Height / cell.RowSpan;

                for (Int32 i = document.Table.Row.Index; i <= document.Table.Row.Index + cell.RowSpan - 1; i++)
                    SetRowHeight(i, normalizedRowHeight);

                cellWidth = CalculateCellWidth(cell.Index, cell.ColSpan);
                var foreBrush = _graphicsCache.GetBrush(style.ForeColor)??XBrushes.Black;
                var backBrush = _graphicsCache.GetBrush(style.BackColor)??XBrushes.White;
                var stringFormat = PdfMeasurementManager.GetXStringFormat(style);

                PdfBorders pdfBorders = new PdfBorders();
                if (style.Border != null)
                {
                    pdfBorders = new PdfBorders(
                        _graphicsCache.GetPen(style.Border[Side.Left]),
                        _graphicsCache.GetPen(style.Border[Side.Right]),
                        _graphicsCache.GetPen(style.Border[Side.Top]),
                        _graphicsCache.GetPen(style.Border[Side.Bottom]));
                }
                
                var pdfCell = new PdfTableCell(_currentLeft, cellWidth, row.Index - 1, row.Index + cell.RowSpan - 2, font, cell.Content, backBrush, foreBrush, stringFormat, pdfBorders);
                _currentRowCells.Add(pdfCell);
            }
            else
                cellWidth = CalculateCellWidth(cell.Index, 1);
            
            _currentLeft += cellWidth;

            return new ValueTask();
        }

        private XUnit CalculateCellWidth(Int32 index, Int32 colSpan)
        {
            XUnit result = XUnit.Zero;

            for (var i = index - 1; i < index + colSpan - 1; i++)
            {
                if (_columnWidthProcessor.ColumnWidths.Length <= i)
                    result += _columnWidthProcessor.ColumnWidths.LastOrDefault();
                else
                    result += _columnWidthProcessor.ColumnWidths[i];
            }

            return result;
        }

        public ValueTask RowEndAsync(Document document)
        {
            return new ValueTask();
        }

        public ValueTask TableEndAsync(Document document)
        {
            var marginLeft = document.Header.Margin[Side.Left].ToXUnit(document.Header.PageWidth);
            var marginTop = document.Header.Margin[Side.Top].ToXUnit(document.Header.PageHeight);

            var currentTop = XUnit.Zero;
            var contentHeight = document.Header.ContentHeight.ToXUnit();

            XGraphics? gfx = null;
            List<(XPoint, XPoint, XPen)> lines = new List<(XPoint, XPoint, XPen)>();
            
            for (int rowIndex = 0; rowIndex < _currentRows.Count; rowIndex++)
            {
                var row = _currentRows[rowIndex];

                var cellsHeight = CalculateCellsHeight(row, out var maxCellHeight);

                if (currentTop + maxCellHeight > contentHeight)
                {
                    DrawLines();
                    gfx = null;
                }

                if (gfx == null)
                {
                    var page = _pdfDocument.AddPage();
                    page.Width = document.Header.PageWidth.ToXUnit();
                    page.Height = document.Header.PageHeight.ToXUnit();
                    gfx = XGraphics.FromPdfPage(page);
                    currentTop = XUnit.Zero;
                }

                for (var index = 0; index < row.Count; index++)
                {
                    var cell = row[index];
                    var top = marginTop + currentTop;
                    var bottom = top + cellsHeight[index];
                    var left = marginLeft + cell.Left;
                    var right = left + cell.Width;

                    var leftTop = new XPoint(left, top);
                    var leftBottom = new XPoint(left, bottom);
                    var rightTop = new XPoint(right, top);
                    var rightBottom = new XPoint(right, bottom);

                    gfx!.DrawPolygon(cell.BackBrush, new[] { leftTop, rightTop, rightBottom, leftBottom, leftTop }, XFillMode.Winding);

                    if (cell.Borders.Top != null)
                        lines.Add((leftTop, rightTop, cell.Borders.Top));

                    if (cell.Borders.Left != null)
                        lines.Add((leftTop, leftBottom, cell.Borders.Left));
                    
                    if (cell.Borders.Bottom != null)
                        lines.Add((leftBottom, rightBottom, cell.Borders.Bottom));

                    if (cell.Borders.Right != null)
                        lines.Add((rightTop, rightBottom, cell.Borders.Right));
                    
                    if (cell.Content != null)
                    {
                        DrawString(gfx, cell.Content, cell.Font, cell.ForeBrush, new XRect(left, top, right - left, bottom - top), cell.StringFormat);
                    }
                }

                currentTop += _rowsHeight[rowIndex];
            }
            
            DrawLines();

            return new ValueTask();

            void DrawLines()
            {
                foreach (var line in lines)
                    gfx!.DrawLine(line.Item3, line.Item1, line.Item2);
                
                lines.Clear();
            }
        }

        private void DrawString(XGraphics gfx, string content, XFont font, XBrush brush, XRect rect, XStringFormat stringFormat)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                var measurement = _measureContext.MeasureString(content, font);

                var lines = content.Split(Environment.NewLine);

                var startX = rect.Left;
                var startY = rect.Top;

                if (stringFormat.Alignment == XStringAlignment.Far)
                    startX = rect.Top - measurement.Width;

                if (stringFormat.LineAlignment == XLineAlignment.Far)
                    startY = rect.Bottom - measurement.Height;

                if (stringFormat.LineAlignment == XLineAlignment.Center)
                    startY = rect.Top + rect.Height / 2.0 - measurement.Height / 2.0;
                
                foreach (var line in lines)
                {
                    var lineMeasurement = _measureContext.MeasureString(line, font);

                    var x = startX;
                    if (stringFormat.Alignment == XStringAlignment.Far)
                        x = rect.Right - lineMeasurement.Width;
                    else if (stringFormat.Alignment == XStringAlignment.Center)
                        x = rect.Left + rect.Width / 2.0 - lineMeasurement.Width / 2.0;
                    
                    gfx.DrawString(line, font, brush, x, startY, XStringFormats.TopLeft);

                    startY += lineMeasurement.Height;
                }
            }
        }

        private XUnit[] CalculateCellsHeight(List<PdfTableCell> row, out XUnit maxCellHeight)
        {
            XUnit[] cellsHeight = new XUnit[row.Count];

            maxCellHeight = XUnit.Zero;
            for (var index = 0; index < row.Count; index++)
            {
                var cell = row[index];
                cellsHeight[index] = XUnit.Zero;

                for (var rowOfMergedCell = cell.RowStart; rowOfMergedCell <= cell.RowEnd; rowOfMergedCell++)
                    cellsHeight[index] += _rowsHeight[rowOfMergedCell];

                if (cellsHeight[index] > maxCellHeight)
                    maxCellHeight = cellsHeight[index];
            }

            return cellsHeight;
        }

        public ValueTask EndAsync(Document document)
        {
            _pdfDocument.Save(_stream);
            return new ValueTask();
        }

        
        
        
    }
}