using System;

namespace Omtv.Api.Primitives
{
    public class Border : SideDriven<BorderSide?>
    {
        public Border MergeFrom(Border newBorderToMerge)
        {
            var result = new Border();
            
            for (Int32 i = 0; i < SideValues.Length; i++)
                result.SideValues[i] = new BorderSide(newBorderToMerge.SideValues[i]?.Thickness ?? SideValues[i]?.Thickness, newBorderToMerge.SideValues[i]?.Color ?? SideValues[i]?.Color);

            return result;
        }
    }

    public class Margin: SideDriven<Measure>
    {
        public Margin()
        {
            SetSide(Side.All, new Measure(15, Unit.Mm));
        }
    }
}