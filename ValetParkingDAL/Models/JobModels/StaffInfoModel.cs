using System;
using System.Collections.Generic;
using System.Text;

namespace ValetParkingDAL.Models.JobModels
{
    public class StaffInfoModel
    {

        public long ParkingLocationId { get; set; }
        public long UserId { get; set; }
        public string DeviceToken { get; set; }


    }
}
