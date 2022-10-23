using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Omtv.Excel
{
    internal class ExcelStylesheetManager
    {
        private readonly Stylesheet _stylesheet;

        private readonly ExcelBorderManager _borderManager;
        private readonly ExcelFontManager _fontManager;
        private readonly ExcelFillManager _fillManager;
        private readonly ExcelStyleFormatManager _styleFormatManager;
        private readonly ExcelCellFormatManager _cellFormatManager;
        private readonly ExcelMeasurementManager _measurementManager;
        
        internal Stylesheet Stylesheet => _stylesheet;
        internal ExcelBorderManager BorderManager => _borderManager;
        internal ExcelFillManager FillManager => _fillManager;
        internal ExcelFontManager FontManager => _fontManager;
        public ExcelStyleFormatManager StyleFormatManager => _styleFormatManager;
        public ExcelCellFormatManager CellFormatManager => _cellFormatManager;

        public ExcelMeasurementManager MeasurementManager => _measurementManager;

        internal ExcelStylesheetManager()
        {
            _borderManager = new ExcelBorderManager();
            _fontManager = new ExcelFontManager();
            _fillManager = new ExcelFillManager();
            _styleFormatManager = new ExcelStyleFormatManager();
            _cellFormatManager = new ExcelCellFormatManager(this);
            _measurementManager = new ExcelMeasurementManager();
            
                
            _stylesheet = new Stylesheet() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "x14ac x16r2 xr" } };
            _stylesheet.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            _stylesheet.AddNamespaceDeclaration("x14ac", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");
            _stylesheet.AddNamespaceDeclaration("x16r2", "http://schemas.microsoft.com/office/spreadsheetml/2015/02/main");
            _stylesheet.AddNamespaceDeclaration("xr", "http://schemas.microsoft.com/office/spreadsheetml/2014/revision");

            _stylesheet.Append(_fontManager.Fonts);
            _stylesheet.Append(_fillManager.Fills);
            _stylesheet.Append(_borderManager.Borders);
            _stylesheet.Append(_styleFormatManager.CellStyleFormats);
            _stylesheet.Append(_cellFormatManager.CellFormats);
            //stylesheet.Append(new CellStyles() { Count = 0 });
            _stylesheet.Append(new DifferentialFormats() { Count = 0 });
            _stylesheet.Append(new TableStyles() { Count = 0 });
            _stylesheet.Append(new StylesheetExtensionList());
        }

    }
}