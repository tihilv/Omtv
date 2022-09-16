using System;
using System.Threading.Tasks;
using System.Xml;
using Omtv.Api.Model;
using Omtv.Api.Primitives;
using Omtv.Api.Processing;

namespace Omtv.Engine.Processing
{
    internal class StyleProcessor : IPartProcessor
    {
        private const String DefaultStyleName = "default";
        
        private const String StyleName = "name";
        private const String ExternalStyleName = "styleName";
        private const String BackColorName = "backColor";
        private const String ForeColorName = "foreColor";
        private const String FontName = "font";

        private const String BorderAllName = "border";
        private const String BorderLeftName = "border.left";
        private const String BorderRightName = "border.right";
        private const String BorderTopName = "border.top";
        private const String BorderBottomName = "border.bottom";
        
        public String Name => "style";
        public Task ProcessAsync(XmlReader reader, ProcessingContext context)
        {
            var style = GetStyle(reader, StyleName);
            if (style != null)
                context.Document.Styles.Add(style.Name!, style);

            return Task.CompletedTask;
        }

        public static Style? GetStyle(XmlReader reader)
        {
            return GetStyle(reader, ExternalStyleName);
        }

        public static Style CombineStyle(ProcessingContext context, Style? newStyleToMerge, Style? parentStyle = null)
        {
            if (newStyleToMerge == null)
            {
                if (parentStyle == null)
                    return context.Document.Styles[DefaultStyleName];
                
                return parentStyle;
            }

            if (String.IsNullOrEmpty(newStyleToMerge.Name))
                return newStyleToMerge;

            if (newStyleToMerge.Name[0] == '+')
            {
                var styleByName = context.Document.Styles[newStyleToMerge.Name.Substring(1)];
                if (parentStyle == null)
                    parentStyle = styleByName;
                else
                    parentStyle = parentStyle.MergeFrom(styleByName);
            }
            else
                parentStyle = context.Document.Styles[newStyleToMerge.Name.Substring(1)];

            return parentStyle.MergeFrom(newStyleToMerge);
        }

        private static Style? GetStyle(XmlReader reader, String styleNameAttr)
        {
            var styleName = reader.GetAttribute(styleNameAttr);
            var backColor = reader.GetAttribute(BackColorName);
            var foreColor = reader.GetAttribute(ForeColorName);
            var font = reader.GetAttribute(FontName);
            
            var borderAll = reader.GetAttribute(BorderAllName);
            var borderLeft = reader.GetAttribute(BorderLeftName);
            var borderRight = reader.GetAttribute(BorderRightName);
            var borderTop = reader.GetAttribute(BorderTopName);
            var borderBottom = reader.GetAttribute(BorderBottomName);

            if (styleName != null || backColor != null || foreColor != null || font != null || borderAll != null || borderLeft != null || borderRight != null || borderTop != null || borderBottom != null)
            {
                var style = new Style(styleName, ColorInfo.Parse(backColor), ColorInfo.Parse(foreColor), FontInfo.Parse(font));
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