using System;

namespace ValetParkingDAL.Models.UserModels
{
    public class CreateOwnerRequest
    {
        public long UserId {get;set;}
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string DeviceToken { get; set; }
        public string DeviceType { get; set; }
        public DateTime CreatedDate {get;set;}
        public DateTime? UpdatedDate {get;set;}
        public long CreatedBy { get; set; }
        public int[] Role {get;set;}
    }
}