using System;
using System.ComponentModel.DataAnnotations;

namespace ValetParkingDAL.Models.UserModels
{
    public class AppVersionModel
    {
        public long? UserId { get; set; }

        [Required]
        public string AppVersionName { get; set; }

        [Required]
        public string AppVersionCode { get; set; }

        [Required]
        public string DeviceType { get; set; }

        public string DeviceName { get; set; }

        public string DeviceToken { get; set; }

        public string OSVersion { get; set; }
        [Required]
        public string TimeZoneId { get; set; }

        public long? ParkingLocationId { get; set; }

        public DateTime? CurrentDate { get; set; }

    }
}