using DocumentFormat.OpenXml.Spreadsheet;

namespace Omtv.Excel
{
    public partial class ExcelTableOutput
    {
        private CellStyleFormats PrepareDefaultCellStyleFormats()
        {
            var cellStyleFormats = new CellStyleFormats() { Count = 1 };
            cellStyleFormats.Append(new CellFormat() { NumberFormatId = 0U, FontId = 0U, FillId = 0U, BorderId = 0U });
            return cellStyleFormats;
        }
    }
}