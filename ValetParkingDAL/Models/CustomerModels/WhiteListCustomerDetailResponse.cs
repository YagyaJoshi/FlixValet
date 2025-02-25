namespace ValetParkingDAL.Models.CustomerModels
{
    public class WhiteListCustomerDetailResponse
    {
        public long WhiteListCustomerId { get; set; }
        public long ParkingBusinessOwnerId { get; set; }
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
        public bool IsActive { get; set; }
    }
}