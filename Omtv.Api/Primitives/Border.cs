using System;
using Omtv.Api.Model;

namespace Omtv.Api.Primitives
{
    public class Border
    {
        private readonly BorderSide[] _borderSides;

        public Border()
        {
            _borderSides = new BorderSide[4];
        }

        internal void SetSide(Side side, BorderSide border)
        {
            if (side == Side.All)
                for (Int32 i = 0; i < _borderSides.Length; i++)
                    _borderSides[i] = border;
            else
                _borderSides[(Int32)side] = border;
        }

        public Border MergeFrom(Border newBorderToMerge)
        {
            var result = new Border();
            
            for (Int32 i = 0; i < _borderSides.Length; i++)
                result._borderSides[i] = new BorderSide(newBorderToMerge._borderSides[i].Thickness ?? _borderSides[i].Thickness, newBorderToMerge._borderSides[i].Color ?? _borderSides[i].Color);

            return result;
        }
    }
}