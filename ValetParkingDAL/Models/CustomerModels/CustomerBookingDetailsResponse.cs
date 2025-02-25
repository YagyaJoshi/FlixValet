using System;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class CustomerBookingDetailsResponse
    {
        public long Id { get; set; }
        public long ParkingLocationId { get; set; }
        public string LocationName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string StateCode { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string ZipCode { get; set; }
        public string LocationPic { get; set; }
        public string Address { get; set; }
        public string Mobile { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public decimal Charges { get; set; }
        public decimal TotalCharges { get; set; }


        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Duration { get; set; }

        public string NumberPlate { get; set; }

        public string VehicleModal { get; set; }
        public string VehicleManufacturer { get; set; }
        public string VehicleColor { get; set; }
        public string VehicleState { get; set; }
        public decimal? TaxAmount { get; set; }

        public long? CustomerVehicleId { get; set; }

        public string MaxStay { get; set; }
        public string BookingMessage { get; set; }
        public decimal ConvenienceFee { get; set; }

        public int BookingCategoryId { get; set; }

        public int? BookingTypeId { get; set; }

    }
}