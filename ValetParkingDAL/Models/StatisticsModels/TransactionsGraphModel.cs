using System.Collections.Generic;

namespace ValetParkingDAL.Models.StatisticsModels
{
    public class TransactionsGraphModel
    {
        public List<TransactionsReport> Report { get; set; }
    }

    public class TransactionsReport
    {

        public string Interval { get; set; }
        public string BookingType { get; set; }
        public long Transactions { get; set; }
    }
}