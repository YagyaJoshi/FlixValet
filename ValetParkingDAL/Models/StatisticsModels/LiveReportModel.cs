using System.Collections.Generic;

namespace ValetParkingDAL.Models.StatisticsModels
{
    public class LiveReportModel
    {
        public EnterExitResponse EnterReport { get; set; }

        public EnterExitResponse ExitReport { get; set; }

        public decimal MonthlyRevenue { get; set; }

        public decimal HourlyRevenue { get; set; }

        public long NoofTransactions { get; set; }

        public long NoofVehiclesEntered { get; set; }
    }

    public class LiveReport
    {
        public string BookingType { get; set; }
        public string Variation { get; set; }
        public int Count { get; set; }
    }

    public class EnterExitResponse
    {
        public List<EnterExitDailyReport> HourlyReport { get; set; }

        public List<EnterExitDailyReport> MonthlyReport { get; set; }

        public int MaxScaleValue { get; set; }

    }

    public class EnterExitDailyReport
    {
        public string Variation { get; set; }
        public int Count { get; set; }

    }

    public class LiveReportDbModel
    {

        public decimal MonthlyRevenue { get; set; }

        public decimal HourlyRevenue { get; set; }

        public long NoofTransactions { get; set; }

        public long NoofVehiclesEntered { get; set; }

        public List<LiveReport> EntryDbReport { get; set; }

        public List<LiveReport> ExitDbReport { get; set; }


    }

}