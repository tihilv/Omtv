using System;
using System.Collections.Generic;

namespace Omtv.Api.Model
{
    internal class SpanStore
    {
        private readonly Table _table;
        private readonly HashSet<SpannedCell> _spannedCells;

        public SpanStore(Table table)
        {
            _table = table;
            _spannedCells = new HashSet<SpannedCell>();
        }

        public Boolean IsSpanned()
        {
            var spannedCell = new SpannedCell(_table.Row.Index, _table.Row.Cell.Index + 1); // next cell to process
            return _spannedCells.Remove(spannedCell);
        }

        public void Register(Byte rowSpan, Byte colSpan)
        {
            if (rowSpan > 1 || colSpan > 1)
            {
                Boolean current = true;
                for (Int32 row = _table.Row.Index; row < _table.Row.Index + rowSpan; row++)
                for (Int32 column = _table.Row.Cell.Index; column < _table.Row.Cell.Index + colSpan; column++)
                {
                    if (current)
                        current = false;
                    else
                    {
                        var spannedCell = new SpannedCell(row, column);
                        if (!_spannedCells.Add(spannedCell))
                            throw new ArgumentException($"Intersection of spanning cells at [{spannedCell}]");
                    }
                }
            }
        }
    }
}