using System.Collections.Generic;

namespace ValetParkingDAL.Models.UserModels
{
    public class StaffMembersResponse
    {
        public List<StaffMember> StaffMembers { get; set; }
        public int Total { get; set; }

    }

    public class StaffMember
    {
        public long UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public bool IsActive { get; set; }
        public string MobileCode { get; set; }
    }
}