using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models
{
    public class BookingDetailsByBIdResponse
    {
        public long BookingId { get; set; }
        public string CustomerName { get; set; }
        public string LocationName { get; set; }
        public decimal Amount { get; set; }
        public decimal ExtraCharges { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string BookingType { get; set; }
        public string PaymentMode { get; set; }
        public string TransactionId { get; set; }
        public bool IsEarlyBirdOfferApplied { get; set; }
        public bool IsNightFareOfferApplied { get; set; }
        public decimal ConvenienceFee { get; set; }
        public int BookingCategoryId { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal UnpaidAmount { get; set; }
        public bool IsCancelled { get; set; }

        public string NumberPlate {  get; set; }
        public List<EnterExit> ValetNotes { get; set; }
    }
    public class EnterExit
    {
        public DateTime EntryDate { get; set; }
        public string Notes { get; set; }
    }
}