using System;

namespace Omtv.Excel
{
    internal static class CellReference
    {
        internal static String Calculate(Int32 row, Int32 column)
        {
            column = column - 1;
            var letterCount = 'Z' - 'A' + 1;
            var columnName = ((Char)('A' + (Byte)(column%letterCount))).ToString();
            if (column >=letterCount)
                columnName = ((Char)('A' + (Byte)(column/letterCount)-1)).ToString() + columnName;
            
            return columnName + row;
        }
    }
}