using System.Collections.Generic;
using ValetParkingDAL.Models.CustomerModels;

namespace ValetParkingDAL.Models.ParkingLocationModels
{
    public class POBusinessOfficeList
    {
        public List<POBusinessOfficeListResponse> Offices { get; set; }
        public int Total { get; set; }
    }

    public class POBusinessOfficeListResponse
    {
        public long BusinessOfficeId { get; set; }
        public string Name { get; set; }

        public string LocationName { get; set; }
        public long ParkingLocationId { get; set; }
        public bool IsActive { get; set; }

        public long UserId { get; set; }
    }
}