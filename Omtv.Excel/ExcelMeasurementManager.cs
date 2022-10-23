using System;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using Omtv.Api.Model;
using Omtv.Api.Primitives;

namespace Omtv.Excel
{
    internal class ExcelMeasurementManager
    {
        internal Double GetExcelWidthInInch(Measure value, Measure pageSize)
        {
            switch (value.Unit)
            {
                case Unit.Pixel:
                    return (value.Value * 0.0909 - 0.63) * 1.03;
                case Unit.Em:
                    return value.Value;
                case Unit.Percent:
                    return GetExcelWidthInInch(new Measure(pageSize.Value * value.Value / 100.0, pageSize.Unit), pageSize);
                case Unit.Mm:
                    return (value.Value - 1.0) / 2.0 + 0.54296875;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal Double GetExcelHeight(Measure value, Measure pageSize)
        {
            switch (value.Unit)
            {
                case Unit.Pixel:
                    return value.Value / 4.0 * 3;
                case Unit.Em:
                    return value.Value;
                case Unit.Percent:
                    return GetExcelHeight(new Measure(pageSize.Value * value.Value / 100.0, pageSize.Unit), pageSize);
                case Unit.Mm:
                    return value.Value / 0.176378 / 4.0 * 3;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal static PageMargins GetPageMargins(Margin margin)
        {
            PageMargins pageMargins = new PageMargins() { Left = 0.7D, Right = 0.7D, Top = 0.75D, Bottom = 0.75D, Header = 0.3D, Footer = 0.3D };

            pageMargins.Left = GetMargin(margin[Side.Left]);
            pageMargins.Right = GetMargin(margin[Side.Right]);
            pageMargins.Top = GetMargin(margin[Side.Top]);
            pageMargins.Bottom = GetMargin(margin[Side.Bottom]);

            return pageMargins;
        }

        private static Double GetMargin(Measure measure)
        {
            switch (measure.Unit)
            {
                case Unit.Em:
                case Unit.Percent:
                case Unit.Pixel:
                    throw new ArgumentException($"Wrong unit of margin: {measure.Unit}");
                case Unit.Mm:
                    return measure.Value / 25.4;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Double GetSize(Measure measure)
        {
            switch (measure.Unit)
            {
                case Unit.Em:
                case Unit.Percent:
                case Unit.Pixel:
                    throw new ArgumentException($"Wrong unit of margin: {measure.Unit}");
                case Unit.Mm:
                    return measure.Value;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private struct SizeTuple
        {
            public readonly UInt32 Width;
            public readonly UInt32 Height;
            public readonly UInt32 Index;

            public SizeTuple(UInt32 width, UInt32 height, UInt32 index)
            {
                Width = width;
                Height = height;
                Index = index;
            }
        }

        private static readonly SizeTuple[] _sizes = new SizeTuple[]
        {
            new SizeTuple(297, 420, 8),
            new SizeTuple(210, 297, 9),
            new SizeTuple(148, 210, 11),
            new SizeTuple(250, 353, 12),
            new SizeTuple(176, 250, 13),
            new SizeTuple(215, 275, 15),
            new SizeTuple(110, 220, 27),
            new SizeTuple(162, 229, 28),
            new SizeTuple(324, 458, 29),
            new SizeTuple(229, 324, 30),
            new SizeTuple(114, 162, 31),
            new SizeTuple(114, 229, 32),
            new SizeTuple(250, 353, 33),
            new SizeTuple(176, 250, 34),
            new SizeTuple(110, 230, 36),
            new SizeTuple(250, 353, 42),
            new SizeTuple(220, 220, 47),
            new SizeTuple(236, 322, 53),
            new SizeTuple(227, 356, 57),
            new SizeTuple(305, 487, 58),
            new SizeTuple(210, 330, 60),
            new SizeTuple(182, 257, 62),
            new SizeTuple(322, 445, 63),
            new SizeTuple(174, 235, 64),
            new SizeTuple(201, 276, 65),
            new SizeTuple(420, 594, 66),

        }.OrderBy(c => c.Height).ThenBy(c => c.Width).ToArray();

        internal static PageSetup GetPageSetup(Document document)
        {
            if (document.Header.ContentWidth.Unit != document.Header.ContentHeight.Unit)
                throw new ArgumentException("Unable to measure page in different units.");

            var width = GetSize(document.Header.ContentWidth);
            var height = GetSize(document.Header.ContentHeight);
            OrientationValues orientation = (width > height) ? OrientationValues.Landscape : OrientationValues.Portrait;

            if (width > height)
                (width, height) = (height, width);

            foreach (var tuple in _sizes)
                if (tuple.Width >= width && tuple.Height >= height)
                    return new PageSetup() { PaperSize = tuple.Index, Orientation = orientation, HorizontalDpi = 0U, VerticalDpi = 0U, Id = $"rId{document.Table.Index}" };

            throw new ArgumentException($"No appropriate page size found.");
        }
    }
}