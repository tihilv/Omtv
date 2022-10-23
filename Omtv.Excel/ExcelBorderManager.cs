using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Spreadsheet;
using Omtv.Api.Primitives;
using Border = DocumentFormat.OpenXml.Spreadsheet.Border;

namespace Omtv.Excel
{
    internal class ExcelBorderManager
    {
        private readonly Borders _borders;

        private readonly Dictionary<BordersTuple, UInt32> _existingBorders = new Dictionary<BordersTuple, UInt32>();

        public Borders Borders => _borders;

        public ExcelBorderManager()
        {
            _borders = new Borders() { Count = 0 };
            GetBorders();
        }

        internal UInt32 GetBorders(Style? tableStyle = null, Style? cellStyle = null, Boolean isLeft = false, Boolean isRight = false, Boolean isTop = false, Boolean isBottom = false)
        {
            var left = isLeft ? tableStyle?.Border?[Side.Left] : cellStyle?.Border?[Side.Left];
            var right = isRight ? tableStyle?.Border?[Side.Right] : cellStyle?.Border?[Side.Right];
            var top = isTop ? tableStyle?.Border?[Side.Top] : cellStyle?.Border?[Side.Top];
            var bottom = isBottom ? tableStyle?.Border?[Side.Bottom] : cellStyle?.Border?[Side.Bottom];

            var bordersTuple = new BordersTuple(left, right, top, bottom);
            if (_existingBorders.TryGetValue(bordersTuple, out var result))
                return result;

            Border borders = new Border();
            LeftBorder leftBorder = new LeftBorder() { Style = GetStyle(bordersTuple.Left) };
            RightBorder rightBorder = new RightBorder() { Style = GetStyle(bordersTuple.Right) };
            TopBorder topBorder = new TopBorder() { Style = GetStyle(bordersTuple.Top) };
            BottomBorder bottomBorder = new BottomBorder() { Style = GetStyle(bordersTuple.Bottom) };
            DiagonalBorder diagonalBorder = new DiagonalBorder();

            borders.Append(leftBorder);
            borders.Append(rightBorder);
            borders.Append(topBorder);
            borders.Append(bottomBorder);
            borders.Append(diagonalBorder);

            _borders.Append(borders);
            result = _borders.Count++;
            _existingBorders.Add(bordersTuple, result);

            return result;

            BorderStyleValues GetStyle(BorderSide? side)
            {
                if (side == null || side.Value.Thickness?.Value == 0)
                    return BorderStyleValues.None;

                if (side.Value.Thickness.Value.Value > 5)
                    return BorderStyleValues.Thick;
                if (side.Value.Thickness.Value.Value > 2)
                    return BorderStyleValues.Medium;

                return BorderStyleValues.Thin;
            }
        }

        struct BordersTuple
        {
            public readonly BorderSide? Left;
            public readonly BorderSide? Right;
            public readonly BorderSide? Top;
            public readonly BorderSide? Bottom;

            public BordersTuple(BorderSide? left, BorderSide? right, BorderSide? top, BorderSide? bottom)
            {
                Left = left;
                Right = right;
                Top = top;
                Bottom = bottom;
            }
        }
    }
}