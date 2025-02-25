using System;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class GuestUserDetailRequest
    {
        public long Id { get; set; }
        public string VehicleName { get; set; }

        public string VehicleModal { get; set; }
        public string VehicleNumber { get; set; }


        public int VehicleColorId { get; set; }

        public long VehicleManufacturerId { get; set; }

        public string StateCode { get; set; }

        public string CountryCode { get; set; }
        public int VehicleTypeId { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}