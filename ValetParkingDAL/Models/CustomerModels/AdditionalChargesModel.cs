using ValetParkingDAL.Models.ParkingLocationModels;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class AdditionalChargesModel
    {
        public decimal PerHourRate { get; set; }

        public decimal UnSettledCharges { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal NewBookingAmount { get; set; }

        public decimal NetAmount { get; set; }

        public decimal OverStayDuration { get; set; }

        // public string BookingType { get; set; }

        public ParkingLocationRateRequest rate { get; set; }

        public double Duration { get; set; }
        public decimal Charges { get; set; }

        public decimal TaxAmountWithConvenienceFee { get; set; }
        public decimal NetAmountWithConvenienceFee { get; set; }
    }
}