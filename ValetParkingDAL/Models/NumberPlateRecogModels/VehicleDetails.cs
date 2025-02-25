namespace ValetParkingDAL.Models.NumberPlateRecogModels
{
    public class VehicleDetails
    {
        public string NumberPlate { get; set; }
        public string VehicleModal { get; set; }
        public long? VehicleColorId { get; set; }
        public string VehicleColor { get; set; }
        public long? VehicleManufacturerId { get; set; }
        public string VehicleManufacturer { get; set; }
        public string StateCode { get; set; }
        public string VehicleCountry { get; set; }
        public string CountryCode { get; set; }
    }
}