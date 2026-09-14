using System;
using System.Collections.Generic;
using Omtv.Api.Primitives;

namespace Omtv.Api.Model
{
    public class ChartSeries
    {
        public String? Header { get; set; }
        public ColorInfo? Color { get; set; }
        public List<Double> Values { get; set; }

        public ChartSeries()
        {
            Values = new List<Double>();
        }

        public ChartSeries(String? header, ColorInfo? color, IEnumerable<Double>? values = null)
        {
            Header = header;
            Color = color;
            Values = values != null ? new List<Double>(values) : new List<Double>();
        }

        public ChartSeries(String? header, IEnumerable<Double>? values = null)
            : this(header, null, values)
        {
        }
    }
}
