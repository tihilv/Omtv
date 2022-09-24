using System;
using System.Linq;

namespace Omtv.Api.Primitives
{
    public class Border : IEquatable<Border>
    {
        private readonly BorderSide[] _borderSides;

        public Border()
        {
            _borderSides = new BorderSide[4];
        }

        private Boolean IsAll
        {
            get
            {
                for (Int32 i = 1; i < _borderSides.Length; i++)
                    if (!_borderSides[0].Equals(_borderSides[i]))
                        return false;

                return true;
            }
        }

        public BorderSide? this[Side side]
        {
            get
            {
                if (side == Side.All)
                {
                    if (IsAll)
                        return this[Side.Left];
                    return null;
                }
                
                return _borderSides[(int)side];
            }
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

        public bool Equals(Border? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _borderSides.SequenceEqual(other._borderSides);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Border)obj);
        }

        public override int GetHashCode()
        {
            return _borderSides.GetHashCode();
        }

        public static bool operator ==(Border? left, Border? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Border? left, Border? right)
        {
            return !Equals(left, right);
        }
    }
}