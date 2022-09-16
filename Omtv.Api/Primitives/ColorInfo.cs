﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;

namespace Omtv.Api.Primitives
{
    public struct ColorInfo
    {
        public readonly Byte A;
        public readonly Byte R;
        public readonly Byte G;
        public readonly Byte B;

        private static readonly Dictionary<String, ColorInfo> _colorsByName;
        private static readonly Dictionary<ColorInfo, String> _nameByColors;

        static ColorInfo()
        {
            _colorsByName = new Dictionary<String, ColorInfo>();
            _nameByColors = new Dictionary<ColorInfo, String>();

            foreach (var member in typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                var color = (Color)member.GetValue(null);
                var colorInfo = new ColorInfo(color.A, color.R, color.G, color.B);
                _colorsByName.Add(member.Name.ToLower(), colorInfo);
                _nameByColors[colorInfo] = member.Name;
            }
        }

        public ColorInfo(Byte a, Byte r, Byte g, Byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public override String ToString()
        {
            if (_nameByColors.TryGetValue(this, out var name))
                return name;

            if (A == 255)
                return $"#{R:X2}{G:X2}{B:X2}";
            return $"#{R:X2}{G:X2}{B:X2}, op.{A:X2}";
        }

        public static ColorInfo? Parse(String? value)
        {
            if (value == null)
                return null;

            var span = value.AsSpan();
            if (span[0] == '#')
            {
                return new ColorInfo(255, Byte.Parse(span[1..3], NumberStyles.HexNumber), Byte.Parse(span[3..5], NumberStyles.HexNumber), Byte.Parse(span[5..7], NumberStyles.HexNumber));
            }

            if (_colorsByName.TryGetValue(value.ToLower(), out var colorInfo))
                return colorInfo;
            
            throw new FormatException($"Unable to parse color '{value}'.");
        }
    }
}