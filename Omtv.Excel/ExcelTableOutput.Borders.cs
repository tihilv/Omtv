using DocumentFormat.OpenXml.Spreadsheet;

namespace Omtv.Excel
{
    public partial class ExcelTableOutput
    {
        private Borders PrepareDefaultBorders()
        {
            var borders = new Borders() { Count = 1 };
            borders.Append(CreateBorder());
            return borders;
        }
        
        private Border CreateBorder()
        {
            Border border = new Border();
            LeftBorder leftBorder1 = new LeftBorder();
            RightBorder rightBorder1 = new RightBorder();
            TopBorder topBorder1 = new TopBorder();
            BottomBorder bottomBorder1 = new BottomBorder();
            DiagonalBorder diagonalBorder1 = new DiagonalBorder();

            border.Append(leftBorder1);
            border.Append(rightBorder1);
            border.Append(topBorder1);
            border.Append(bottomBorder1);
            border.Append(diagonalBorder1);
            return border;
        }
    }
}