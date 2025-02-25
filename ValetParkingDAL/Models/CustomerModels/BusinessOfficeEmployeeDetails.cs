namespace ValetParkingDAL.Models.CustomerModels
{
    public class BusinessOfficeEmployeeDetails
    {
        public long BusinessOfficeEmployeeId { get; set; }
        public long BusinessOfficeId { get; set; }
        public long CustomerVehicleId { get; set; }
        public int OfficeDuration { get; set; }
        public long CustomerInfoId { get; set; }
        public string NumberPlate { get; set; }
        public string VehicleModal { get; set; }
        public int? VehicleColorId { get; set; }
        public string VehicleColor { get; set; }
        public long? VehicleManufacturerId { get; set; }
        public string VehicleManufacturer { get; set; }
        public string VehicleState { get; set; }
        public string StateCode { get; set; }
        public string VehicleCountry { get; set; }
        public string CountryCode { get; set; }
        public int? VehicleTypeId { get; set; }
    }
}