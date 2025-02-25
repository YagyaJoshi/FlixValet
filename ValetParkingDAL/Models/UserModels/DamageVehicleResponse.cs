using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.UserModels
{
    public class DamageVehicleResponse
    {
        public List<DamageVehicle> DamageVehicleList { get; set; }
        public int Total { get; set; }
    }

    public class DamageVehicle
    {
        public long DamageVehicleId { get; set; }
        public string ValetName { get; set; }
        public string CustomerName { get; set; }
        public string NumberPlate { get; set; }
        public DateTime ReportedDate { get; set; }
    }
}