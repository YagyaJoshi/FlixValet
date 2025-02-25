using System.Collections.Generic;

namespace ValetParkingDAL.Models.StateModels
{
    public class StateCodeRequest
    {
        public List<States> lstStates { get; set; }
        public string CountryCode { get; set; }
    }
}