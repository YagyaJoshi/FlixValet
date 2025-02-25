namespace ValetParkingDAL.Models.CustomerModels
{
    public class GuestParkingResponse
    {
        public long Id { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public string LocationPic { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public int No_of_Spaces { get; set; }
        public string LocationName { get; set; }
        public long ParkingBusinessOwnerId { get; set; }
        public decimal TotalDuration {get;set;}
        public decimal BookingAmount {get;set;}
        public decimal NetAmount {get;set;}
        public string VehicleNumber {get;set;}
        public string VehicleModal {get;set;}
        public string VehicleManufacturer {get;set;}

    }
}