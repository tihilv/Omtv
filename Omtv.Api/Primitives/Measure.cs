using System;
using System.Collections.Generic;
using System.Globalization;

namespace Omtv.Api.Primitives
{
    public struct Measure
    {
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

        public static Measure Parse(String value, Unit? defaultUnit = null)
        {
            var span = value.AsSpan().Trim();
            
            foreach(var v in _units)
                if (span.EndsWith(v.Key))
                    return new Measure(Double.Parse(span.Slice(0, span.Length - v.Key.Length).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture), v.Value);

            if (defaultUnit != null)
                return new Measure(Double.Parse(span.Trim(),NumberStyles.Float, CultureInfo.InvariantCulture), defaultUnit.Value);

            throw new FormatException($"Unable to parse measure '{value}'.");
        }
    }
}