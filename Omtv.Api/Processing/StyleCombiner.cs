using Omtv.Api.Model;
using Omtv.Api.Primitives;

namespace Omtv.Api.Processing
{
    public static class StyleCombiner
    {
        public static Style CombineStyles(Style? style, Style? newStyleToMerge, Document document)
        {
            if (style == null)
                style = document.Styles[Style.DefaultStyleName];

            if (newStyleToMerge == null)
                return style.Unwrap(document.Styles);

            return style.Unwrap(document.Styles).MergeFrom(newStyleToMerge.Unwrap(document.Styles), false);
        }
    }
}