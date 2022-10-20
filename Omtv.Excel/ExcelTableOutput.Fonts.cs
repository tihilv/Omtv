using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Omtv.Api.Primitives;

namespace Omtv.Excel
{
    public partial class ExcelTableOutput
    {
        private Fonts _fonts;

        private readonly Dictionary<FontTuple, UInt32> _existingFonts = new Dictionary<FontTuple, UInt32>();

        private Fonts PrepareDefaultFonts()
        {
            _fonts = new Fonts() { Count = 0 };
            GetFont(null);
            return _fonts;
        }
        
        private UInt32 GetFont(Style? style)
        {
            var fontTuple = new FontTuple(style);
            if (_existingFonts.TryGetValue(fontTuple, out var result))
                return result;
            
            Font font = new Font();
            FontSize fontSize = new FontSize(){ Val = fontTuple.Size };
            Color color = new Color();
            if (!String.IsNullOrEmpty(fontTuple.Color))
                color.Rgb = HexBinaryValue.FromString(fontTuple.Color);
            else
                color.Theme = 1U;

            FontName fontName = new FontName() { Val = fontTuple.Name };

            font.Append(fontSize);
            font.Append(color);
            font.Append(fontName);
            
            _fonts.Append(font);
            result = _fonts.Count++;
            _existingFonts.Add(fontTuple, result);

            return result;
        }

        struct FontTuple
        {
            public readonly String Name;
            public readonly Double Size;
            public readonly String? Color;

            public FontTuple(Style? style)
            {
                Name = style?.Font?.Family ?? "Calibri";
                Size = style?.Font?.Size.Value?? 11D;
                var c = style?.ForeColor;
                if (c != null)
                    Color = $"{c.Value.A:X2}{c.Value.R:X2}{c.Value.G:X2}{c.Value.B:X2}";
                else
                    Color = null;
            }
        }
    }
}