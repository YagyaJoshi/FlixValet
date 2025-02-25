using System;

namespace ValetParkingDAL.Models
{
    public class LocationCameraSettings
    {
        public long LocationCameraId { get; set; }
        public long ParkingLocationId { get; set; }
        public string CameraId { get; set; }
        public bool IsForEntry { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}