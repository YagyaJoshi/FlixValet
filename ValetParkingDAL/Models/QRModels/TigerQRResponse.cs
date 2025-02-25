using System;
using System.Collections.Generic;

namespace ValetParkingDAL.Models.QRModels
{
    public class TigerQRResponse
    {

        public Data data { get; set; }
        public string imageUrl { get; set; }
        public string qrId { get; set; }
        public string qrUrl { get; set; }
    }
   
    public class Lock
    {
        public bool active { get; set; }
    }

    public class Data
    {
        public Lock @lock { get; set; }
        public bool archive { get; set; }
        public bool bulk { get; set; }
        public int scans { get; set; }
        public int scanCounter { get; set; }
        public bool scanLoop { get; set; }
        public object expiryDate { get; set; }
        public object expiryScans { get; set; }
        public string _id { get; set; }
        public string qrId { get; set; }
        public string qrType { get; set; }
        public string qrCategory { get; set; }
        public string shortUrl { get; set; }
        public string redirectUrl { get; set; }
        public List<object> murlData { get; set; }
        public string qrName { get; set; }
        public string qrImage { get; set; }
        public string owner { get; set; }
        public List<object> scanData { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public int __v { get; set; }
        public object user { get; set; }
        public string id { get; set; }
    }


}