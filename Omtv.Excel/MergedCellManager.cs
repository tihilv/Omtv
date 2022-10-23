using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Table = Omtv.Api.Model.Table;

namespace Omtv.Excel
{
    internal class MergedCellManager
    {
        private readonly Worksheet _worksheet;

        private readonly List<String> _mergeReferences;

        public MergedCellManager(Worksheet worksheet)
        {
            _worksheet = worksheet;
            _mergeReferences = new List<String>();
        }

        internal void MergeCellsIfNeeded()
        {
            var mergeCells = _worksheet.Elements<MergeCells>().First();
            if (!_mergeReferences.Any())
            {
                _worksheet.RemoveChild(mergeCells);
                return;
            }

            foreach (var reference in _mergeReferences)
            {
                MergeCell mergeCell = new MergeCell() { Reference = new StringValue(reference) };
                mergeCells.Append(mergeCell);
            }
            _mergeReferences.Clear();
        }

        public void AddMergedCellsIfNeeded(Table table)
        {
            var row = table.Row;
            var cell = row.Cell;

            if (cell.RowSpan > 1 || cell.ColSpan > 1)
                _mergeReferences.Add(CellReference.Calculate(row.Index, cell.Index) + ":" + CellReference.Calculate(row.Index + row.Cell.RowSpan - 1, row.Cell.Index + row.Cell.ColSpan - 1));
        }
    }
}