using System.Collections.Generic;

namespace ValetParkingDAL.Models.StatisticsModels
{
    public class OccupancyGraphModel
    {
        List<OccupancyReport> Report { get; set; }
    }
    public class OccupancyReport
    {
        public string BookingType { get; set; }
        public string Variation { get; set; }
        public int OccupancyCount { get; set; }
    }

    public class OccupancyDailyResponse
    {
        public List<OccupancyDailyReport> HourlyReport { get; set; }

        public List<OccupancyDailyReport> MonthlyReport { get; set; }

    }

    public class OccupancyDailyReport
    {
        public string Variation { get; set; }
        public int OccupancyCount { get; set; }

    }
}