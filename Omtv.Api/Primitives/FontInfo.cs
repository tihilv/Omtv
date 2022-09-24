using System;

namespace Omtv.Api.Primitives
{
    public struct FontInfo : IEquatable<FontInfo>
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
                var size = Measure.ParseExact(restLine[0], Unit.Em);

                return new FontInfo(fontFamily.ToString(), size);
            }

            return null;
        }

        public Boolean Equals(FontInfo other)
        {
            return Family == other.Family && Size.Equals(other.Size);
        }

        public override Boolean Equals(Object? obj)
        {
            return obj is FontInfo other && Equals(other);
        }

        public override Int32 GetHashCode()
        {
            unchecked
            {
                return (Family.GetHashCode() * 397) ^ Size.GetHashCode();
            }
        }

        public static Boolean operator ==(FontInfo left, FontInfo right)
        {
            return left.Equals(right);
        }

        public static Boolean operator !=(FontInfo left, FontInfo right)
        {
            return !left.Equals(right);
        }
    }
}