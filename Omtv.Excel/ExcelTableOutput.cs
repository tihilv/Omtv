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
using Document = Omtv.Api.Model.Document;

namespace Omtv.Excel
{
    public partial class ExcelTableOutput: ITableOutput
    {
        private readonly Stream _stream;
        
        private readonly Dictionary<String, Int32> _texts;
        private readonly List<String> _mergeReferences;

        private SpreadsheetDocument _spreadsheetDocument;
        private WorkbookPart _workbookPart;
        private WorksheetPart _worksheetPart;
        private Sheets _sheets;
        private SharedStringTablePart _shareStringPart;
        private WorkbookStylesPart _workbookStylesPart;

        private Sheet _sheet;
        private Row _row;

        private readonly List<(CellFormat, Style)> _lastRowCellFormats;
        
        public ExcelTableOutput(Stream stream)
        {
            _stream = stream;

            _texts = new Dictionary<String, Int32>();
            _mergeReferences = new List<String>();
            _lastRowCellFormats = new List<(CellFormat, Style)>();
        }

        public async ValueTask StartAsync(Document document)
        {
            _spreadsheetDocument = SpreadsheetDocument.Create(_stream, SpreadsheetDocumentType.Workbook);
            //_spreadsheetDocument.AddCoreFilePropertiesPart();

            _workbookPart = _spreadsheetDocument.AddWorkbookPart();
            _workbookPart.Workbook = new Workbook();

            _shareStringPart = _workbookPart.AddNewPart<SharedStringTablePart>();
            _shareStringPart.SharedStringTable = new SharedStringTable();

            _workbookStylesPart = _workbookPart.AddNewPart<WorkbookStylesPart>();
            _workbookStylesPart.Stylesheet = PrepareStylesheet();
            
            _sheets = _spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
        }
        
        public async ValueTask TableStartAsync(Document document)
        {
            _worksheetPart = _workbookPart.AddNewPart<WorksheetPart>();
            _worksheetPart.Worksheet = new Worksheet(new Columns(), new SheetData());

            _sheet = new Sheet() { Id = _spreadsheetDocument.WorkbookPart.GetIdOfPart(_worksheetPart), SheetId = (UInt32)document.Table.Index, Name = document.Table.Name };
            _sheets.Append(_sheet);

            SetupColumns(document);
        }

        private void SetupColumns(Document document)
        {
            var columns = _worksheetPart.Worksheet.GetFirstChild<Columns>();
            if (document.Table.Columns.Any())
            {
                for (var index = 0; index < document.Table.Columns.Count; index++)
                {
                    var column = document.Table.Columns[index];
                    if (column.Width != null)
                    {
                        Column excelColumn = new Column();
                        excelColumn.Min = (UInt32)(index);
                        excelColumn.Max = (UInt32)(index);
                        excelColumn.Width = GetExcelWidth(column.Width.Value, document.Header.ContentWidth);
                        excelColumn.CustomWidth = true;
                        columns.Append(excelColumn);
                    }
                }
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
                _row.Height = GetExcelHeight(document.Table.Row.Height.Value, document.Header.ContentHeight);
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
                newCell.CellValue = new CellValue(GetCellTextIndex(cell.Content).ToString());
                newCell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                
                var style = document.Table.Row.Cell.GetCombinedStyle();
                var borderId = GetBorders(document.Table.GetCombinedStyle(), style, cell.Index == 1, isTop: row.Index == 1);
                newCell.StyleIndex = GetCellPropertiesIndex(newCell, style, borderId, out var cellFormat);
                _lastRowCellFormats.Add((cellFormat, style));

                if (cell.RowSpan > 1 || cell.ColSpan > 1)
                    _mergeReferences.Add(newCell.CellReference.Value + ":" + GetCellReference(row.Index + row.Cell.RowSpan - 1, row.Cell.Index + row.Cell.ColSpan - 1));
            }
        }

        private Int32 GetCellTextIndex(String text)
        {
            if (_texts.TryGetValue(text, out var index))
                return index;

            index = _shareStringPart.SharedStringTable.Elements<SharedStringItem>().Count();
            _texts.Add(text, index);
            _shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new Text(text)));
            return index;
        }

        private String GetCellReference(Int32 row, Int32 column)
        {
            column = column - 1;
            var letterCount = 'Z' - 'A' + 1;
            var columnName = ((Char)('A' + (Byte)(column%letterCount))).ToString();
            if (column >=letterCount)
                columnName = ((Char)('A' + (Byte)(column/letterCount)-1)).ToString() + columnName;
            
            return columnName + row;
        }

        private Cell CreateCell(Int32 row, Int32 column)
        {
            var reference = GetCellReference(row, column);
            Cell newCell = new Cell() { CellReference = reference };
            _row.InsertAt(newCell, column - 1);
            return newCell;
        }
        
        public async ValueTask RowEndAsync(Document document)
        {
            var lastCellFormat = _lastRowCellFormats.LastOrDefault();
            if (lastCellFormat.Item1 != null)
            {
                lastCellFormat.Item1.BorderId = GetBorders(document.Table.GetCombinedStyle(), lastCellFormat.Item2,
                    _lastRowCellFormats.Count == 1, isTop: document.Table.Row.Index == 1, isRight: true);
            }
        }

        public async ValueTask TableEndAsync(Document document)
        {
            for (var i = 0; i < _lastRowCellFormats.Count; i++)
            {
                var pair = _lastRowCellFormats[i];
                pair.Item1.BorderId = GetBorders(document.Table.GetCombinedStyle(), pair.Item2,
                    i == 0, isTop: document.Table.Row.Index == 1, isRight: i == _lastRowCellFormats.Count-1, isBottom: true);
            }

            MergeCellsIfNeeded();
            _worksheetPart.Worksheet.Save();
        }
        
        private void MergeCellsIfNeeded()
        {
            var worksheet = _worksheetPart.Worksheet;
            MergeCells mergeCells;
            if (worksheet.Elements<MergeCells>().Any())
                mergeCells = worksheet.Elements<MergeCells>().First();
            else
            {
                mergeCells = new MergeCells();

                // Insert a MergeCells object into the specified position.
                if (worksheet.Elements<CustomSheetView>().Any())
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<CustomSheetView>().First());
                else if (worksheet.Elements<DataConsolidate>().Any())
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<DataConsolidate>().First());
                else if (worksheet.Elements<SortState>().Any())
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<SortState>().First());
                else if (worksheet.Elements<AutoFilter>().Any())
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<AutoFilter>().First());
                else if (worksheet.Elements<Scenarios>().Any())
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<Scenarios>().First());
                else if (worksheet.Elements<ProtectedRanges>().Any())
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<ProtectedRanges>().First());
                else if (worksheet.Elements<SheetProtection>().Any())
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<SheetProtection>().First());
                else if (worksheet.Elements<SheetCalculationProperties>().Any())
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<SheetCalculationProperties>().First());
                else
                    worksheet.InsertAfter(mergeCells, worksheet.Elements<SheetData>().First());
            }

            foreach (var reference in _mergeReferences)
            {
                MergeCell mergeCell = new MergeCell() { Reference = new StringValue(reference) };
                mergeCells.Append(mergeCell);
            }
            _mergeReferences.Clear();
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