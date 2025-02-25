using System.Collections.Generic;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.StateModels;

namespace ValetParkingBLL.Interfaces
{
    public interface IRegion
    {
        void AddStateCodesForCountry(List<States> lstStates, string CountryCode);

        List<TimeZone> GetTimeZones();


        List<States> GetStatesForCountry(string CountryCode);

        List<Countries> GetAllCountries();
    }
}