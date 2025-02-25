using System.Collections.Generic;

namespace ValetParkingDAL.Models.StatisticsModels
{
    public class RevenueGraphModel
    {
        public List<RevenueReport> Report { get; set; }
    }

    public class RevenueReport
    {

        public string Interval { get; set; }
        public string BookingType { get; set; }
        public decimal Amount { get; set; }
    }

    

}
