using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Omtv.Excel
{
    public partial class ExcelTableOutput
    {
        private Stylesheet PrepareStylesheet()
        {
            var stylesheet = new Stylesheet() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "x14ac x16r2 xr" } };
            stylesheet.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            stylesheet.AddNamespaceDeclaration("x14ac", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");
            stylesheet.AddNamespaceDeclaration("x16r2", "http://schemas.microsoft.com/office/spreadsheetml/2015/02/main");
            stylesheet.AddNamespaceDeclaration("xr", "http://schemas.microsoft.com/office/spreadsheetml/2014/revision");

            stylesheet.Append(PrepareDefaultFonts());
            stylesheet.Append(PrepareDefaultFills());
            stylesheet.Append(PrepareDefaultBorders());
            stylesheet.Append(PrepareDefaultCellStyleFormats());
            stylesheet.Append(PrepareDefaultCellFormats());
            //stylesheet.Append(new CellStyles() { Count = 0 });
            stylesheet.Append(new DifferentialFormats() { Count = 0 });
            stylesheet.Append(new TableStyles() { Count = 0 });
            stylesheet.Append(new StylesheetExtensionList());

            return stylesheet;
        }

    }
}