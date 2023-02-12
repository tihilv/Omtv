using System.Collections.Generic;
using Omtv.Api.Primitives;
using PdfSharpCore.Drawing;

namespace Omtv.Pdf
{
    internal class GraphicsCache
    {
        private readonly Dictionary<ColorInfo, XBrush> _brushes;
        private readonly Dictionary<(ColorInfo, Measure), XPen> _pens;

        public GraphicsCache()
        {
            _brushes = new Dictionary<ColorInfo,XBrush>();
            _pens = new Dictionary<(ColorInfo, Measure), XPen>();
        }

        public XBrush? GetBrush(ColorInfo? foreColor)
        {
            if (foreColor == null)
                return null;

            if (!_brushes.TryGetValue(foreColor.Value, out var result))
            {
                result = new XSolidBrush(GetColor(foreColor.Value));
                _brushes.Add(foreColor.Value, result);
            }

            return result;
        }

        public XPen? GetPen(BorderSide? borderSide)
        {
            return GetPen(borderSide?.Color, borderSide?.Thickness);
        }
        
        public XPen? GetPen(ColorInfo? colorInfo, Measure? thickness)
        {
            if (thickness == null || thickness.Value.Value == 0)
                return null;

            var colorInfoReal = colorInfo ?? new ColorInfo(255, 0, 0, 0);
            
            var key = (colorInfoReal, thickness.Value);
            
            if (!_pens.TryGetValue(key, out var result))
            {
                result = new XPen(GetColor(colorInfoReal), thickness.Value.ToXUnit());
                _pens.Add(key, result);
            }

            return result;
        }
        
        private static XColor GetColor(ColorInfo colorInfo)
        {
            return XColor.FromArgb(colorInfo.A, colorInfo.R, colorInfo.G, colorInfo.B);
        }
    }
}