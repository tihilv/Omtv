using System;
using System.Collections.Generic;
using Omtv.Api.Primitives;
using PdfSharpCore.Drawing;

namespace Omtv.Pdf
{
    internal class FontCache
    {
        private readonly Dictionary<(FontInfo, Boolean), XFont> _cachedFonts;

        public FontCache()
        {
            _cachedFonts = new Dictionary<(FontInfo, Boolean), XFont>();
        }

        public XFont GetFont(FontInfo fontInfo, Boolean isHeader)
        {
            var fontSize = fontInfo.Size.ToXUnit();
            var key = (fontInfo, isHeader);
            if (!_cachedFonts.TryGetValue(key, out var result))
            {
                result = new XFont(fontInfo.Family, fontSize, isHeader ? XFontStyle.Bold : XFontStyle.Regular);
                _cachedFonts.Add(key, result);
            }

            return result;
        }
    }
}