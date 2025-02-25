using System.Collections.Generic;

namespace ValetParkingDAL.Models.StatisticsModels
{
    public class KeyStatistics_PieGraphModel
    {
        public List<UserTypePieModel> UserTypeReport { get; set; }

        public List<BookingTypePieModel> BookingTypeReport { get; set; }
    }

    public class UserTypePieModel
    {
        public string UserType { get; set; }

        public decimal Revenue { get; set; }

        public decimal BookingPercentage { get; set; }

    }

    public class BookingTypePieModel
    {
        public string BookingType { get; set; }

        public decimal Revenue { get; set; }

        public decimal BookingPercentage { get; set; }

    }
}