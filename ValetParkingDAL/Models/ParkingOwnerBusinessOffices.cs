using System;

namespace ValetParkingDAL.Models
{
    public class ParkingOwnerBusinessOffices
    {
        public long BusinessOfficeId { get; set; }
        public long ParkingLocationId { get; set; }
        public string Name { get; set; }
        public string Email {  get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public long ModifyBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class POBusinessOffice
    {
        public long Id { get; set; }

        public string Email { get; set; }

        public string LocationName { get; set; }
    }
}