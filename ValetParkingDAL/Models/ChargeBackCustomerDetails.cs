using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models
{
    public class ChargeBackCustomerDetails
    {
        public string BookingType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string NumberPlate { get; set; }
        public string Duration { get; set; }
        public decimal NetAmount { get; set; }
        public string LocationName { get; set; }
        public string CustomerName { get; set; }
        
        public string Email {  get; set; }
    }

    public class ChargeBackBookingList
    {
        public List<ChargeBackBookingReport> List { get; set; }
        public int Total { get; set; }
    }

    public class ChargeBackBookingReport
    {
        public long Id { get; set; }

        public string Url { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}