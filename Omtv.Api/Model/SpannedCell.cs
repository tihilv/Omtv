using System;

namespace Omtv.Api.Model
{
    internal struct SpannedCell : IEquatable<SpannedCell>
    {
        public readonly Int32 Row;
        public readonly Int32 Column;

        public SpannedCell(Int32 row, Int32 column)
        {
            Row = row;
            Column = column;
        }

        public bool Equals(SpannedCell other)
        {
            return Row == other.Row && Column == other.Column;
        }

        public override bool Equals(object? obj)
        {
            return obj is SpannedCell other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Row * 397) ^ Column;
            }
        }

        public static bool operator ==(SpannedCell left, SpannedCell right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SpannedCell left, SpannedCell right)
        {
            return !left.Equals(right);
        }
    }
}