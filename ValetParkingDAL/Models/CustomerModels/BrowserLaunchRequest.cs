using System;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class BrowserLaunchRequest
    {
        public long UserId { get; set; }
        public string BrowserDeviceToken { get; set; }
        public DateTime CurrentDate {get;set;}
    }
}