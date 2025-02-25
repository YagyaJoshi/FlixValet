using System;

namespace ValetParkingDAL.Models.StatisticsModels
{
    public class BookingListModel
    {
        public string BookingType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string NumberPlate { get; set; }
        public string Duration { get; set; }
        public decimal NetAmount { get; set; }
    }
}