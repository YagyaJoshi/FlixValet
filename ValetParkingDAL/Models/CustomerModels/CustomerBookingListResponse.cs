using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class CustomerBookingListResponse
    {
        public List<CustomerBookingList> CustomerBookingList { get; set; }
        public int Total { get; set; }
    }

    public class CustomerBookingList
    {
        public long Id { get; set; }
        public string Mobile { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public decimal Duration { get; set; }
        public string LocationName { get; set; }
        public string LocationPic { get; set; }
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string QRCodePath  { get; set; }


    }
}