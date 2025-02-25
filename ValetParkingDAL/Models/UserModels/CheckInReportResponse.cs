using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.UserModels
{
    public class CheckInReportResponse
    {
        public List<ActiveUserResponse> ActiveUsers { get; set; }
        public List<InActiveUserResponse> InActiveUsers { get; set; }

    }
    public class ActiveUserResponse
    {
        public long Id { get; set; }
        public string StaffName { get; set; }
        public string Mobile { get; set; }
        public string CheckInTime { get; set; }
        public string LastCheckOut { get; set; }
        public bool IsActive { get; set; }
    }
    public class InActiveUserResponse
    {
        public long Id { get; set; }
        public string StaffName { get; set; }
        public string Mobile { get; set; }
        public string CheckOutTime { get; set; }
        public string LastCheckInTime { get; set; }
        public bool IsActive { get; set; }
    }
}