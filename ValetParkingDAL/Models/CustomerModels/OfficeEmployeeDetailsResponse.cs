using System.Collections.Generic;

namespace ValetParkingDAL.Models.CustomerModels
{

    public class POBusinessOfficeEmployeeList
    {
        public List<OfficeEmployeeDetailsResponse> EmployeeList { get; set; }
        public int Total { get; set; }
    }
    public class OfficeEmployeeDetailsResponse
    {
        public long BusinessOfficeEmployeeId { get; set; }
        public long BusinessOfficeId { get; set; }
        public long CustomerVehicleId { get; set; }
        public long ParkingLocationId { get; set; }
        public string NumberPlate { get; set; }
        public long CustomerInfoId { get; set; }
        public int OfficeDuration { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public string MobileCode { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }

    }
}