using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.UserModels
{
    public class CheckInCheckOut
    {

        public List<CheckInOut> CheckInOutDetails { get; set; }
        public int Total { get; set; }
    }

    public class CheckInOut
    {
        public DateTime CheckInTime { get; set; }
        public dynamic CheckOutTime { get; set; }

    }
}