using System.Collections.Generic;
using ValetParkingDAL.Models.StateModels;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class VehicleMasterResponse
    {
        public List<VehicleManufacturerMst> ListManufacturer { get; set; }

        public List<VehicleColorMst> ListColor { get; set; }

        public List<VehicleTypeMst> ListVehicleType { get; set; }

        public List<Countries> ListCountries { get; set; }

        public List<StatesMst> ListStates { get; set; }
    }
}