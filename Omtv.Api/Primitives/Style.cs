using System;
using System.Collections.Generic;
using System.Linq;

namespace Omtv.Api.Primitives
{
    public class Style
    {
        public const String DefaultStyleName = "default";
        
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

        public Style MergeFrom(Style newStyleToMerge, Boolean preserveParents, Boolean overwriteBorder)
        {
            var result = new Style(null,
                newStyleToMerge.BackColor ?? BackColor,
                newStyleToMerge.ForeColor ?? ForeColor,
                newStyleToMerge.Font ?? Font,
                newStyleToMerge.HorizontalAlignment ?? HorizontalAlignment,
                newStyleToMerge.VerticalAlignment ?? VerticalAlignment,
                preserveParents ? ((newStyleToMerge.Parents ?? Array.Empty<String>()).Union(Parents ?? Array.Empty<String>()).Distinct().ToArray()) : null
            );
            
            if (Border == null || overwriteBorder)
                result.Border = newStyleToMerge.Border;
            else if (newStyleToMerge.Border == null)
                result.Border = Border;
            else
                result.Border = Border.MergeFrom(newStyleToMerge.Border);

            return result;
        }

        public Style Unwrap(Dictionary<String, Style> styles)
        {
            Style? result = null;
            if (Parents != null)
                foreach (var parentName in Parents)
                {
                    if (styles.TryGetValue(parentName, out var parent))
                    {
                        if (result == null)
                            result = parent;
                        else
                            result = result.MergeFrom(parent, false, false);
                    }
                }

            if (result == null)
                result = this;
            else
                result = result.MergeFrom(this, false, false);

            return result;
        }
    }
}