using System;
using System.Threading.Tasks;
using System.Xml;
using Omtv.Api.Primitives;
using Omtv.Api.Processing;

namespace Omtv.Engine.Processing
{
    internal class StyleProcessor : IPartProcessor
    {
        private const String StyleName = "name";
        private const String ParentStylesName = "styles";
        private const String BackColorName = "backColor";
        private const String ForeColorName = "foreColor";
        private const String FontName = "font";
        private const String HorizontalAlignmentName = "align";
        private const String VerticalAlignmentName = "valign";

        private const String BorderAllName = "border";
        private const String BorderLeftName = "border.left";
        private const String BorderRightName = "border.right";
        private const String BorderTopName = "border.top";
        private const String BorderBottomName = "border.bottom";
        
        public String Name => "style";
        public async ValueTask ProcessAsync(XmlReader reader, ProcessingContext context)
        {
            var style = GetStyle(reader);
            if (style != null)
                context.Document.Styles.Add(style.Name!, style);
        }

        
        
        public static Style? GetStyle(XmlReader reader)
        {
            var styleName = reader.GetAttribute(StyleName);
            var parents = reader.GetAttribute(ParentStylesName)?.Split(";", StringSplitOptions.RemoveEmptyEntries);
            var backColor = reader.GetAttribute(BackColorName);
            var foreColor = reader.GetAttribute(ForeColorName);
            var font = reader.GetAttribute(FontName);
            var horizontalAlign = AlignmentExtensions.Parse(reader.GetAttribute(HorizontalAlignmentName));
            var verticalAlign = AlignmentExtensions.Parse(reader.GetAttribute(VerticalAlignmentName));
            
            var borderAll = reader.GetAttribute(BorderAllName);
            var borderLeft = reader.GetAttribute(BorderLeftName);
            var borderRight = reader.GetAttribute(BorderRightName);
            var borderTop = reader.GetAttribute(BorderTopName);
            var borderBottom = reader.GetAttribute(BorderBottomName);

            if (styleName != null || backColor != null || foreColor != null || font != null || horizontalAlign != null || verticalAlign != null ||
                borderAll != null || borderLeft != null || borderRight != null || borderTop != null || borderBottom != null || (parents != null && parents.Length > 0))
            {
                var style = new Style(styleName, ColorInfo.Parse(backColor), ColorInfo.Parse(foreColor), FontInfo.Parse(font), horizontalAlign, verticalAlign, parents);
                style.SetBorder(Side.All, BorderSide.Parse(borderAll));
                style.SetBorder(Side.Left, BorderSide.Parse(borderLeft));
                style.SetBorder(Side.Right, BorderSide.Parse(borderRight));
                style.SetBorder(Side.Top, BorderSide.Parse(borderTop));
                style.SetBorder(Side.Bottom, BorderSide.Parse(borderBottom));
                return style;
            }

            return null;
        }
    }
}