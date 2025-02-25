using System;
namespace ValetParkingAPI.Models
{
    public class Response
    {
        public bool Status { get; set; }
        public dynamic Data { get; set; }
        public string Message { get; set; }
    }

}