using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Omtv.Api.Primitives;
using Omtv.Api.Processing;
using Omtv.Api.Utils;
using Document = Omtv.Api.Model.Document;

namespace Omtv.Excel
{
    public class ExcelTableOutput: ITableOutput
    {
        private readonly Stream _stream;
        
        private SpreadsheetDocument _spreadsheetDocument;
        private WorkbookPart _workbookPart;
        private WorksheetPart _worksheetPart;
        private Sheets _sheets;
        private SharedStringTablePart _shareStringPart;

        private readonly ExcelStylesheetManager _stylesheetManager;
        private readonly ExcelTextManager _textManager;
        private MergedCellManager _mergeCellManager;
        
        private Row _row;

        private readonly List<(CellFormat, Style)> _lastRowCellFormats;

        public ExcelTableOutput(Stream stream)
        {
            _stream = stream;
            _stylesheetManager = new ExcelStylesheetManager();
            _textManager = new ExcelTextManager();

            _lastRowCellFormats = new List<(CellFormat, Style)>();
        }

        public async ValueTask StartAsync(Document document)
        {
            _spreadsheetDocument = SpreadsheetDocument.Create(_stream, SpreadsheetDocumentType.Workbook);

            _workbookPart = _spreadsheetDocument.AddWorkbookPart();
            _workbookPart.Workbook = new Workbook();

            _shareStringPart = _workbookPart.AddNewPart<SharedStringTablePart>();
            _shareStringPart.SharedStringTable = _textManager.SharedStringTable;

            var workbookStylesPart = _workbookPart.AddNewPart<WorkbookStylesPart>();
            workbookStylesPart.Stylesheet = _stylesheetManager.Stylesheet;
            
            _sheets = _spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
        }
        
        public async ValueTask TableStartAsync(Document document)
        {
            _worksheetPart = _workbookPart.AddNewPart<WorksheetPart>();
            _worksheetPart.Worksheet = new Worksheet(new Columns(), new SheetData(), new MergeCells(), ExcelMeasurementManager.GetPageMargins(document.Header.Margin), ExcelMeasurementManager.GetPageSetup(document));

            var name = document.Table.Name;
            if (String.IsNullOrEmpty(name))
                name = $"Table {document.Table.Index}";
            
            var sheet = new Sheet() { Id = _spreadsheetDocument.WorkbookPart.GetIdOfPart(_worksheetPart), SheetId = (UInt32)document.Table.Index, Name = name };
            _sheets.Append(sheet);

            _mergeCellManager = new MergedCellManager(_worksheetPart.Worksheet);
            SetupColumns(document);
        }

        private void SetupColumns(Document document)
        {
            var columns = _worksheetPart.Worksheet.GetFirstChild<Columns>();
            
            var columnProcessor = new ColumnWidthProcessor(document, 
                measure => _stylesheetManager.MeasurementManager.GetExcelWidthInInch(measure, document.Header.ContentWidth));
            
                for (var index = 0; index < columnProcessor.ColumnWidths.Length; index++)
                {
                    var width = columnProcessor.ColumnWidths[index];

                    Column excelColumn = new Column();
                    excelColumn.Min = (UInt32)(index + 1);
                    excelColumn.Max = (UInt32)(index + 1);
                    excelColumn.Width = width;
                    excelColumn.CustomWidth = true;
                    columns.Append(excelColumn);
                }

            if (!columns.HasChildren)
                _worksheetPart.Worksheet.RemoveChild(columns);
        }

        public async ValueTask RowStartAsync(Document document)
        {
            _lastRowCellFormats.Clear();
            _row = new Row() { RowIndex = (UInt32)document.Table.Row.Index };
            if (document.Table.Row.Height != null)
            {
                _row.Height = _stylesheetManager.MeasurementManager.GetExcelHeight(document.Table.Row.Height.Value, document.Header.ContentHeight);
                _row.CustomHeight = true;
            }

            SheetData sheetData = _worksheetPart.Worksheet.GetFirstChild<SheetData>();
            sheetData.Append(_row);
        }

        public async ValueTask CellAsync(Document document)
        {
            var row = document.Table.Row;
            var cell = row.Cell;
            var newCell = CreateCell(row.Index, cell.Index);
            
            if (!cell.Spanned && cell.Content != null)
            {
                newCell.CellValue = new CellValue(_textManager.GetCellTextIndex(cell.Content).ToString());
                newCell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                
                var style = document.Table.Row.Cell.GetCombinedStyle();
                var borderId = _stylesheetManager.BorderManager.GetBorders(document.Table.GetCombinedStyle(), style, cell.Index == 1, isTop: row.Index == 1);
                newCell.StyleIndex = _stylesheetManager.CellFormatManager.GetCellPropertiesIndex(newCell, style, borderId, cell.Content.Contains(Environment.NewLine), out var cellFormat);
                _lastRowCellFormats.Add((cellFormat, style));

                _mergeCellManager.AddMergedCellsIfNeeded(document.Table);
            }
        }

        private Cell CreateCell(Int32 row, Int32 column)
        {
            var reference = CellReference.Calculate(row, column);
            Cell newCell = new Cell() { CellReference = reference };
            _row.InsertAt(newCell, column - 1);
            return newCell;
        }
        
        public async ValueTask RowEndAsync(Document document)
        {
            var lastCellFormat = _lastRowCellFormats.LastOrDefault();
            if (lastCellFormat.Item1 != null)
            {
                lastCellFormat.Item1.BorderId = _stylesheetManager.BorderManager.GetBorders(document.Table.GetCombinedStyle(), lastCellFormat.Item2,
                    _lastRowCellFormats.Count == 1, isTop: document.Table.Row.Index == 1, isRight: true);
            }
        }

        public async ValueTask TableEndAsync(Document document)
        {
            for (var i = 0; i < _lastRowCellFormats.Count; i++)
            {
                var pair = _lastRowCellFormats[i];
                pair.Item1.BorderId = _stylesheetManager.BorderManager.GetBorders(document.Table.GetCombinedStyle(), pair.Item2,
                    i == 0, isTop: document.Table.Row.Index == 1, isRight: i == _lastRowCellFormats.Count-1, isBottom: true);
            }

            _mergeCellManager.MergeCellsIfNeeded();
            _worksheetPart.Worksheet.Save();
        }
        
        public async ValueTask EndAsync(Document document)
        {
            _shareStringPart.SharedStringTable.Save();
            _workbookPart.Workbook.Save();
            _spreadsheetDocument.Close();
            await _stream.FlushAsync();
        }
    }
}