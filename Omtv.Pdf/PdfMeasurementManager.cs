using System;
using Omtv.Api.Primitives;
using PdfSharpCore.Drawing;

namespace Omtv.Pdf
{
    internal static class PdfMeasurementManager
    {
        internal static XUnit ToXUnit(this Measure measure, Measure mainSize)
        {
            if (measure.Unit == Unit.Percent)
                return ToXUnit(new Measure(mainSize.Value * measure.Value / 100.0, mainSize.Unit));

            return ToXUnit(measure);
        }
        
        internal static XUnit ToXUnit(this Measure measure)
        {
            switch (measure.Unit)
            {
                case Unit.Mm:
                    return XUnit.FromMillimeter(measure.Value);
                case Unit.Pixel:
                    return XUnit.FromPoint(measure.Value);
                case Unit.Em:
                    return XUnit.FromPresentation(measure.Value);
                case Unit.Percent:
                default:
                    throw new ArgumentException($"Wrong unit of margin: {measure.Unit}");
            }
        }
        
        internal static XStringFormat GetXStringFormat(Style style)
        {
            var hAlign = style.HorizontalAlignment ?? Alignment.Before;
            var vAlign = style.VerticalAlignment ?? Alignment.After;

            if (vAlign == Alignment.Before && hAlign == Alignment.Before)
                return XStringFormats.TopLeft;
            if (vAlign == Alignment.Before && hAlign == Alignment.Center)
                return XStringFormats.TopCenter;
            if (vAlign == Alignment.Before && hAlign == Alignment.After)
                return XStringFormats.TopRight;
            
            if (vAlign == Alignment.Center && hAlign == Alignment.Before)
                return XStringFormats.CenterLeft;
            if (vAlign == Alignment.Center && hAlign == Alignment.Center)
                return XStringFormats.Center;
            if (vAlign == Alignment.Center && hAlign == Alignment.After)
                return XStringFormats.CenterRight;
            
            if (vAlign == Alignment.After && hAlign == Alignment.Before)
                return XStringFormats.BottomLeft;
            if (vAlign == Alignment.After && hAlign == Alignment.Center)
                return XStringFormats.BottomCenter;
            if (vAlign == Alignment.After && hAlign == Alignment.After)
                return XStringFormats.BottomRight;

            throw new ArgumentException($"Unknown alignment pair: horizontal: {vAlign}, vertical: {hAlign}");
        }

    }
}