using System.Collections.Generic;

namespace ValetParkingDAL.Models.ParkingLocationModels
{

    public class ParkingLocationsResponse
    {
        public List<LocationsList> ParkingLocations {get;set;}
        public int Total {get;set;}

    }
    public class LocationsList
    {
        public long Id { get; set; }
        public string LocationName { get; set; }
        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }


        public string Country { get; set; }


        public int No_of_Spaces { get; set; }


    }
}