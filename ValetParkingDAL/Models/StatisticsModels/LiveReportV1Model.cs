namespace ValetParkingDAL.Models.StatisticsModels
{
    public class LiveReportV1Model
    {
        public GraphResponseModel EnterReport { get; set; }

        public GraphResponseModel ExitReport { get; set; }

        public int MaxValueEnterReport { get; set; }
        public int MaxValueExitReport { get; set; }

        public decimal MonthlyRevenue { get; set; }

        public decimal HourlyRevenue { get; set; }

        public long NoofTransactions { get; set; }

        public long NoofVehiclesEntered { get; set; }
    }
}