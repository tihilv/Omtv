using System;
using System.Collections.Generic;
using System.Globalization;

namespace Omtv.Api.Primitives
{
    public struct Measure : IEquatable<Measure>
    {
        public static readonly Measure Null = new Measure(0, Unit.Pixel);
        
        public readonly Unit Unit;
        public readonly Double Value;

        public Measure(Double value, Unit unit)
        {
            Unit = unit;
            Value = value;
        }

        private static readonly Dictionary<String, Unit> _units = new Dictionary<String, Unit>()
        {
            ["%"] = Unit.Percent,
            ["px"] = Unit.Pixel,
            ["em"] = Unit.Em,
            ["mm"] = Unit.Mm
        };

        public override String ToString()
        {
            return $"{Value} {Unit}";
        }

        public static Measure ParseExact(String value, Unit? defaultUnit = null)
        {
            var result = Parse(value, defaultUnit);
            if (result == null)
                throw new ArgumentNullException();

            return result.Value;
        }
        
        public static Measure? Parse(String? value, Unit? defaultUnit = null)
        {
            if (value == null)
                return null;
            
            var span = value.AsSpan().Trim();
            
            foreach(var v in _units)
                if (span.EndsWith(v.Key))
                    return new Measure(Double.Parse(span.Slice(0, span.Length - v.Key.Length).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture), v.Value);

            if (defaultUnit != null)
                return new Measure(Double.Parse(span.Trim(),NumberStyles.Float, CultureInfo.InvariantCulture), defaultUnit.Value);

            throw new FormatException($"Unable to parse measure '{value}'.");
        }

        public bool Equals(Measure other)
        {
            return Unit == other.Unit && Value.Equals(other.Value);
        }

        public override bool Equals(object? obj)
        {
            return obj is Measure other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Unit * 397) ^ Value.GetHashCode();
            }
        }

        public static bool operator ==(Measure left, Measure right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Measure left, Measure right)
        {
            return !left.Equals(right);
        }
    }
}