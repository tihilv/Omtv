using System;
using System.Linq;

namespace Omtv.Api.Primitives
{
    public abstract class SideDriven<T> : IEquatable<SideDriven<T>>
    {
        protected readonly T[] SideValues;

        protected SideDriven()
        {
            SideValues = new T[4];
        }

        private Boolean IsAll
        {
            get
            {
                for (Int32 i = 1; i < SideValues.Length; i++)
                {
                    if (SideValues[0] == null && SideValues[i] != null)
                        return false;
                    
                    if (SideValues[0] != null && !SideValues[0]!.Equals(SideValues[i]))
                        return false;
                }

                return true;
            }
        }

        public T? this[Side side]
        {
            get
            {
                if (side == Side.All)
                {
                    if (IsAll)
                        return this[Side.Left];
                    return default(T);
                }
                
                return SideValues[(Int32)side];
            }
        }

        internal void SetSide(Side side, T sideValue)
        {
            if (side == Side.All)
                for (Int32 i = 0; i < SideValues.Length; i++)
                    SideValues[i] = sideValue;
            else
                SideValues[(Int32)side] = sideValue;
        }

        public Boolean Equals(SideDriven<T>? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return SideValues.SequenceEqual(other.SideValues);
        }

        public override Boolean Equals(Object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Border)obj);
        }

        public override Int32 GetHashCode()
        {
            return SideValues.GetHashCode();
        }

        public static Boolean operator ==(SideDriven<T>? left, SideDriven<T>? right)
        {
            return Equals(left, right);
        }

        public static Boolean operator !=(SideDriven<T>? left, SideDriven<T>? right)
        {
            return !Equals(left, right);
        }
    }
}