using System;

namespace Omtv.Api.Primitives
{
    public struct BorderSide : IEquatable<BorderSide>
    {
        public readonly Measure? Thickness;
        public readonly ColorInfo? Color;

        public BorderSide(Measure? thickness, ColorInfo? color)
        {
            Thickness = thickness;
            Color = color;
        }

        public static BorderSide? Parse(String? border)
        {
            if (border == null)
                return null;

            var parts = border.Split(' ');

            ColorInfo? color = null;
            Measure? thickness = null;

            if (parts.Length > 0 && parts[0] != "-")
                thickness = Measure.Parse(parts[0], Unit.Pixel);

            if (parts.Length > 1)
                color = ColorInfo.Parse(parts[1]);

            return new BorderSide(thickness, color);
        }

        public Boolean Equals(BorderSide other)
        {
            return Nullable.Equals(Thickness, other.Thickness) && Nullable.Equals(Color, other.Color);
        }

        public override Boolean Equals(Object? obj)
        {
            return obj is BorderSide other && Equals(other);
        }

        public override Int32 GetHashCode()
        {
            unchecked
            {
                return (Thickness.GetHashCode() * 397) ^ Color.GetHashCode();
            }
        }

        public static Boolean operator ==(BorderSide left, BorderSide right)
        {
            return left.Equals(right);
        }

        public static Boolean operator !=(BorderSide left, BorderSide right)
        {
            return !left.Equals(right);
        }
    }
}

