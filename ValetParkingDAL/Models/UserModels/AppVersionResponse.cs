namespace ValetParkingDAL.Models.UserModels
{
    public class AppVersionResponse
    {
        public string AppVersionCode { get; set; }
        public bool IsMandatoryUpdate { get; set; }
        public string UpdateMessage { get; set; }
        public long BadgeCount { get; set; }
    }
}