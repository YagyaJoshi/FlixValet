namespace ValetParkingDAL.Models.QRModels
{
    public class DynamicQRCodeModel
    {
        public long ParkingLocationId { get; set; }
        public string LogoUrl { get; set; }

        public bool IsMonthly { get; set; } = false;
    }
}