using System.Security.Cryptography.X509Certificates;

namespace ValetParkingDAL.Models.ParkingLocationModels
{
    public class ParkingLocationName
    {
        public long Id { get; set; }
        public string LocationName { get; set; }

        public string Currency { get; set; }

        public string CurrencySymbol { get; set; }

        public string QrCodePath { get; set; }

        public string QRCodePathMonthly { get; set; }

        public string TimeZone {  get; set; }
    }

    public class ParkingLocationBasicDetails
    {
        public long Id { get; set; }
        public string LocationName { get; set; }

        public string QrCodePath { get; set; }

        public string QRCodePathMonthly { get; set; }

        public string TimeZone { get; set; }
    }
}