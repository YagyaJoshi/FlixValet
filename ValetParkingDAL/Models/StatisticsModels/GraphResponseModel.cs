namespace ValetParkingDAL.Models.StatisticsModels
{
    public class GraphResponseModel
    {
        public string[] Categories { get; set; }

        public decimal[] Hourly { get; set; }

        public decimal[] Monthly { get; set; }

    }
}