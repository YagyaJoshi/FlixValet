using System.Collections.Generic;

namespace ValetParkingDAL.Models.ParkingLocationModels
{
    public class QRListResponse
    {
        public List<LocationQRData> ListQrs { get; set; }

        public long Total { get; set; }
    }

    public class LocationQRData
    {

        public long Id { get; set; }
        public string LocationName { get; set; }
        public string QRCodePath { get; set; }
    }
}