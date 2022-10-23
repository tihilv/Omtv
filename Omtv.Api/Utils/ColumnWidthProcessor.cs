using System;
using System.Linq;
using Omtv.Api.Model;
using Omtv.Api.Primitives;

namespace Omtv.Api.Utils
{
    public class ColumnWidthProcessor
    {
        private readonly Func<Measure, Double> _convertValueToUnitFunc;
        private readonly Document _document;

        private Double _fixedSizeInUnit;
        private Double _percentSize;
        private Int32 _undefinedWidthColumnCount;

        private Double[] _columnWidths;

        public Double[] ColumnWidths => _columnWidths;

        public ColumnWidthProcessor(Document document, Func<Measure, Double> convertValueToUnitFunc)
        {
            _columnWidths = Array.Empty<Double>();
            
            _document = document;
            _convertValueToUnitFunc = convertValueToUnitFunc;
            SetupColumns();
        }

        private void SetupColumns()
        {
            if (_document.Table.Columns.Any())
            {
                var newColumnWidths = new Double?[_document.Table.Columns.Count];
                
                for (var index = 0; index < _document.Table.Columns.Count; index++)
                {
                    var column = _document.Table.Columns[index];
                    if (column.Width != null)
                    {
                        if (column.Width.Value.Unit == Unit.Percent)
                            _percentSize += column.Width.Value.Value;
                        else
                        {
                            var sizeInInch = _convertValueToUnitFunc(column.Width.Value);
                            _fixedSizeInUnit += sizeInInch;
                            newColumnWidths[index] = sizeInInch;
                        }
                    }
                    else
                        _undefinedWidthColumnCount++;
                }

                var freeWidthInInch = _convertValueToUnitFunc(_document.Header.ContentWidth) - _fixedSizeInUnit;
                var freePercentPerUndefinedColumn = Math.Round((100 - Math.Min(_percentSize, 100)) / _undefinedWidthColumnCount,2);

                _columnWidths = new Double[newColumnWidths.Length];
                for (var index = 0; index < _document.Table.Columns.Count; index++)
                {
                    var column = _document.Table.Columns[index];
                    var width = newColumnWidths[index];
                    if (width == null)
                    {
                        if (column?.Width?.Unit == Unit.Percent)
                            width = freeWidthInInch * column.Width.Value.Value / 100;
                        else
                            width = freeWidthInInch * freePercentPerUndefinedColumn / 100;
                    }

                    _columnWidths[index] = width.Value;
                }
            }
        }
    }
}