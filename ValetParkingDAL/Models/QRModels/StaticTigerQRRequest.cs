namespace ValetParkingDAL.Models.QRModels
{
    public class StaticTigerQRRequest
    {

        public int size { get; set; }
        public string colorDark { get; set; }
        public bool gradient { get; set; }
        public string grdType { get; set; }
        public string logo { get; set; }
        public string eye_outer { get; set; }
        public string eye_inner { get; set; }
        public string qrFormat { get; set; }
        public string qrData { get; set; }
        public string backgroundColor { get; set; }
        public string color01 { get; set; }
        public string color02 { get; set; }
        public bool transparentBkg { get; set; }
        public int frame { get; set; }
        public string frameColor { get; set; }
        public string qrCategory { get; set; }
        public string text { get; set; }

        public string frameText { get; set; }
    }
}