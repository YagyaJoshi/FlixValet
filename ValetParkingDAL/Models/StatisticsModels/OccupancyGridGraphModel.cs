using System.Collections.Generic;

namespace ValetParkingDAL.Models.StatisticsModels
{
    public class OccupancyGridGraphModel
    {
        public List<BookingListModel> BookingList { get; set; }

        public int Total { get; set; }

        public dynamic OccupancyReport { get; set; }


        public int MaxScaleValue { get; set; }

    }
}