using System;
using System.Collections.Generic;
using ValetParkingDAL.Models.UserModels;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class DamageVehicleReport
    {

        public long DamageVehicleId { get; set; }
        public string ValetName { get; set; }
        public string CustomerName { get; set; }
        public string Mobile { get; set; }
        public DateTime ReportedOn { get; set; }
        public string Notes { get; set; }
        public string NumberPlate { get; set; }
        public string VehicleModal { get; set; }
        public List<DamageVehicleImages> Images { get; set; }
    }

}