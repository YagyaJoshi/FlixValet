using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ValetParkingDAL.Models.CustomerModels
{
    public class GuestList
    {
        public Guest WithNumber { get; set; }
        public Guest WithoutNumber { get; set; }

    }
    public class Guest
    {
        public long CustomerId { get; set; }
        public long CustomerVehicleId { get; set; }

        [JsonIgnore]
        public bool HasMobile { get; set; }

        public string Mobile { get; set; }

        public string MobileCode { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class GuestListv1
    {
        public List<Guest> WithNumber { get; set; }
        public Guest WithoutNumber { get; set; }

    }
}