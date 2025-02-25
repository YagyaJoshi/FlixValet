using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models
{
    public class AccountReceivableReportResponse
    {
        public List<BookingDetailList> Bookings { get; set; }
        public int TotalBookings { get; set; }

        public decimal TotalAmount { get; set; }

    }
    public class BookingDetailList
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
        public int CurrencyId { get; set; }
        public long TimeZoneId { get; set; }
    }
}