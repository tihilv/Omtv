using System;
using System.Collections.Generic;

namespace Omtv.Api.Model
{
    public class Chart
    {
        public ChartType Type { get; set; }
        public Boolean ShowLegend { get; set; }
        public String? Title { get; set; }
        public List<String> Categories { get; set; }
        public List<ChartSeries> Series { get; set; }

        public Chart(ChartType type = ChartType.Bar, Boolean showLegend = false)
        {
            Type = type;
            ShowLegend = showLegend;
            Categories = new List<String>();
            Series = new List<ChartSeries>();
        }

        public Chart(ChartType type, Boolean showLegend, IEnumerable<String>? categories, IEnumerable<ChartSeries>? series)
        {
            Type = type;
            ShowLegend = showLegend;
            Categories = categories != null ? new List<String>(categories) : new List<String>();
            Series = series != null ? new List<ChartSeries>(series) : new List<ChartSeries>();
        }
    }
}
