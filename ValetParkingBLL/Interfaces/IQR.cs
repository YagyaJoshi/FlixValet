using System;
using ValetParkingDAL.Models;

namespace ValetParkingBLL.Interfaces
{
    public interface IQR
    {
        public string GetDynamicTigerQRImage(string Text, string LogoUrl);
        public string GetStaticTigerQRImage(string Text);

        public dynamic GetCompressedStaticTigerQRImage(string Text, string LogoUrl);

        public QRCodeDataResponse GetQrCodeScanData(string qrId, string timeZone, DateTime date);
    }
}