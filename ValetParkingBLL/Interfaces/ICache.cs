using System.Collections.Generic;
using ValetParkingDAL.Models.CustomerModels;
using TimeZone = ValetParkingDAL.Models.TimeZone;

namespace ValetParkingBLL.Interfaces
{
    public interface ICache
    {
        List<TimeZone> CachedTimeZones();

        VehicleMasterResponse CachedVehicleMasterData();
    }
}