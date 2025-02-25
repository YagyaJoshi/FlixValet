using System.Collections.Generic;

namespace ValetParkingDAL.Models.UserModels
{
    public class CameraSettingListResponse
    {
        public List<CameraSettingList> CameraSettingList { get; set; }
        public int Total { get; set; }
    }
    public class CameraSettingList
    {
        public long Id { get; set; }
        public string LocationName { get; set; }
        public string CameraId { get; set; }
        public bool IsForEntry { get; set; }
    }
}