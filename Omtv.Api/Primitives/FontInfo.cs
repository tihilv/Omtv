using System;

namespace Omtv.Api.Primitives
{
    public struct FontInfo
    {
        public readonly String Family;
        public readonly Measure Size;

        public FontInfo(String family, Measure size)
        {
            Family = family;
            Size = size;
        }

        public static FontInfo? Parse(String? font)
        {
            if (!String.IsNullOrEmpty(font))
            {
                var span = font.AsSpan();
                var lastQuoteIndex = span.LastIndexOf('\'');
                var fontFamily = span.Slice(1, lastQuoteIndex - 1);
                var restLine = span.Slice(lastQuoteIndex+1).Trim().ToString().Split(' ');
                var size = Measure.Parse(restLine[0], Unit.Em);

                return new FontInfo(fontFamily.ToString(), size);
            }

            return null;
        }
    }
}