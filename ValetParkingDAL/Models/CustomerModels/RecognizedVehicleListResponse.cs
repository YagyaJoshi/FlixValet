using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.CustomerModels
{


    public class RecognizedVehicleListResponse
    {

        public List<RecognizedVehicleListByOwner> RecognizedVehicleList { get; set; }
        public int Total { get; set; }
    }

    public class RecognizedVehicleListByOwner
    {

        public long Id { get; set; }
        public string NumberPlate { get; set; }
        public DateTime ReportedDate { get; set; }
        public long? CustomerBookingId { get; set; }
        public string CustomerType { get; set; }
        public string CustomerName { get; set; }

    }
}
