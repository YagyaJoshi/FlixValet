using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.ParkingLocationModels
{

    public class BookingsByOwnerResponse
    {

        public List<BookingList> Bookings { get; set; }
        public int Total { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal HourlyRevenue { get; set; }
        public int TotalMonthlyBookings { get; set; }
        public int TotalHourlyBookings { get; set; }
        public decimal TotalConvenienceFee { get; set; }

    }
    public class BookingList
    {
        public long BookingId { get; set; }
        public string CustomerName { get; set; }
        public string LocationName { get; set; }
        public decimal TotalAmount { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string NumberPlate { get; set; }
        public string BookingType { get; set; }
        public decimal UnpaidAmount { get; set; }
        public bool IsCancelled { get; set; }
        public decimal ConvenienceFee { get; set; }
    }
}