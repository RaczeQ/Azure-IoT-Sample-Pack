using System;

namespace IoTChartPage.Models
{
    public class IoTData
    {
        public DateTime time { get; set; }
        public string deviceid { get; set; }
        public double temperature { get; set; }
        public double humidity { get; set; }
    }
}