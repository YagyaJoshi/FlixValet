using System;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class BookingDetailsByIdResponse
    {
        public long Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public decimal BookingAmount { get; set; }
        public decimal ExtraCharges { get; set; }
        public decimal TotalCharges { get; set; }

        public string NumberPlate { get; set; }
    }
}