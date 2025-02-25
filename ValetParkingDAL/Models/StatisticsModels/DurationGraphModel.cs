using System.Collections.Generic;

namespace ValetParkingDAL.Models.StatisticsModels
{
    public class DurationGraphModel
    {
        public List<BookingListModel> BookingList { get; set; }

        public int Total { get; set; }

        public GraphResponseModel DurationReport { get; set; }
    }

    public class DurationReport
    {

        public int Interval { get; set; }
        public string BookingType { get; set; }
        public long BookingsCount { get; set; }
    }


}