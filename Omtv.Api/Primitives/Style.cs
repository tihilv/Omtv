using System;

namespace Omtv.Api.Primitives
{
    public class Style
    {
        public String? Name { get; }
        public ColorInfo? BackColor { get; }
        public ColorInfo? ForeColor { get; }
        public FontInfo? Font { get; }
        public Alignment? HorizontalAlignment { get; }
        public Alignment? VerticalAlignment { get; }
        public Border? Border { get; private set; }
        public String[]? Parents { get; }

        public Style(String? name, ColorInfo? backColor, ColorInfo? foreColor, FontInfo? font, Alignment? horizontalAlignment = null, Alignment? verticalAlignment = null, String[]? parents = null)
        {
            Name = name;
            BackColor = backColor;
            ForeColor = foreColor;
            Font = font;
            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;
            Parents = parents;
        }

        internal void SetBorder(Side side, BorderSide? borderSide)
        {
            if (borderSide != null)
            {
                if (Border == null)
                    Border = new Border();
                
                Border.SetSide(side, borderSide.Value);
            }
        }

        public Style MergeFrom(Style newStyleToMerge)
        {
            var result = new Style(null, newStyleToMerge.BackColor ?? BackColor, newStyleToMerge.ForeColor ?? ForeColor, newStyleToMerge.Font ?? Font);
            if (Border == null)
                result.Border = newStyleToMerge.Border;
            else if (newStyleToMerge.Border == null)
                result.Border = Border;
            else
                result.Border = Border.MergeFrom(newStyleToMerge.Border);

            return result;
        }
    }
}