using DocumentFormat.OpenXml.Spreadsheet;

namespace Omtv.Excel
{
    internal class ExcelStyleFormatManager
    {
        private readonly CellStyleFormats _cellStyleFormats;

        public CellStyleFormats CellStyleFormats => _cellStyleFormats;

        internal ExcelStyleFormatManager()
        {
            _cellStyleFormats = new CellStyleFormats() { Count = 1 };
            _cellStyleFormats.Append(new CellFormat() { NumberFormatId = 0U, FontId = 0U, FillId = 0U, BorderId = 0U });
        }
    }
}