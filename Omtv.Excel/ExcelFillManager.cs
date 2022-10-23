using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Omtv.Api.Primitives;

namespace Omtv.Excel
{
    internal class ExcelFillManager
    {
        private readonly Fills _fills;

        private readonly Dictionary<FillTuple, UInt32> _existingFills = new Dictionary<FillTuple, UInt32>();

        public Fills Fills => _fills;

        internal ExcelFillManager()
        {
            _fills = new Fills() { Count = 0 };
            GetFill(null, PatternValues.None);
            GetFill(null, PatternValues.Gray125);
        }
        
        internal UInt32 GetFill(Style? style, PatternValues defaultPattern = PatternValues.None)
        {
            var fillTuple = new FillTuple(style, defaultPattern);
            if (_existingFills.TryGetValue(fillTuple, out var result))
                return result;
            
            var fill = new Fill();
            
            PatternFill patternFill = new PatternFill();
            if (!String.IsNullOrEmpty(fillTuple.Color))
            {
                patternFill.PatternType = PatternValues.Solid;
                patternFill.Append(new ForegroundColor { Rgb = HexBinaryValue.FromString(fillTuple.Color) });
                patternFill.Append(new BackgroundColor { Indexed = 64U });
            }
            else
                patternFill.PatternType = defaultPattern;

            fill.Append(patternFill);
            
            _fills.Append(fill);
            result = _fills.Count++;
            _existingFills.Add(fillTuple, result);

            return result;
        }

        struct FillTuple
        {
            public readonly String? Color;
            public readonly PatternValues Pattern;

            public FillTuple(Style style, PatternValues defaultPattern)
            {
                if (style == null)
                    Pattern = defaultPattern;
                else
                    Pattern = PatternValues.Solid;

                var c = style?.BackColor;
                if (c != null)
                    Color = $"{c.Value.A:X2}{c.Value.R:X2}{c.Value.G:X2}{c.Value.B:X2}";
                else
                    Color = null;
            }
        }
    }
}