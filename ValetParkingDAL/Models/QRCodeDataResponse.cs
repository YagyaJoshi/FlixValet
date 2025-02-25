using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ValetParkingDAL.Models
{
    public class QrData
    {
        [JsonPropertyName("totalUniqueScanByDate")]
        public int TotalUniqueScanByDate { get; set; }

        [JsonPropertyName("data")]
        public List<ScanData> Data { get; set; }

        [JsonPropertyName("smediaActivity")]
        public List<object> SmediaActivity { get; set; }

        [JsonPropertyName("scans")]
        public int Scans { get; set; }

        [JsonPropertyName("totalUniqueScan")]
        public int TotalUniqueScan { get; set; }

        [JsonPropertyName("device")]
        public List<Device> Device { get; set; }

        [JsonPropertyName("unique")]
        public List<Unique> Unique { get; set; }

        [JsonPropertyName("graph")]
        public List<Graph> Graph { get; set; }

        [JsonPropertyName("totalScanByDate")]
        public int TotalScanByDate { get; set; }

        [JsonPropertyName("campaign")]
        public Campaign Campaign { get; set; }

        [JsonPropertyName("country")]
        public List<Country> Country { get; set; }

        [JsonPropertyName("city")]
        public List<City> City { get; set; }
    }

    public class QRCodeDataResponse
    {
        [JsonPropertyName("data")]
        public QrData Data { get; set; }
    }

    public class ScanData
    {
        [JsonPropertyName("_id")]
        public Id Id { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class Id
    {
        [JsonPropertyName("device")]
        public string Device { get; set; }

        [JsonPropertyName("loc")]
        public string Loc { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }
    }

    public class Device
    {
        [JsonPropertyName("_id")]
        public Id Id { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class Unique
    {
        [JsonPropertyName("_id")]
        public UniqueId Id { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class UniqueId
    {
        [JsonPropertyName("month")]
        public int Month { get; set; }

        [JsonPropertyName("day")]
        public int Day { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }
    }

    public class Graph
    {
        [JsonPropertyName("_id")]
        public UniqueId Id { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class Campaign
    {
        [JsonPropertyName("lock")]
        public Lock Lock { get; set; }

        [JsonPropertyName("watchList")]
        public WatchList WatchList { get; set; }

        [JsonPropertyName("gpsTrackData")]
        public GpsTrackData GpsTrackData { get; set; }

        [JsonPropertyName("archive")]
        public bool Archive { get; set; }

        [JsonPropertyName("members")]
        public List<object> Members { get; set; }

        [JsonPropertyName("bulk")]
        public bool Bulk { get; set; }

        [JsonPropertyName("scans")]
        public int Scans { get; set; }

        [JsonPropertyName("scanCounter")]
        public int ScanCounter { get; set; }

        [JsonPropertyName("previousScan")]
        public int PreviousScan { get; set; }

        [JsonPropertyName("todayScan")]
        public int TodayScan { get; set; }

        [JsonPropertyName("weekScans")]
        public List<int> WeekScans { get; set; }

        [JsonPropertyName("previousWeekScans")]
        public List<int> PreviousWeekScans { get; set; }

        [JsonPropertyName("scanLoop")]
        public bool ScanLoop { get; set; }

        [JsonPropertyName("expiryDate")]
        public object ExpiryDate { get; set; }

        [JsonPropertyName("expiryScans")]
        public object ExpiryScans { get; set; }

        [JsonPropertyName("expireByIP")]
        public bool ExpireByIP { get; set; }

        [JsonPropertyName("fileIds")]
        public object FileIds { get; set; }

        [JsonPropertyName("blocked")]
        public bool Blocked { get; set; }

        [JsonPropertyName("groupIds")]
        public List<object> GroupIds { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("qrId")]
        public string QrId { get; set; }

        [JsonPropertyName("qrType")]
        public string QrType { get; set; }

        [JsonPropertyName("qrCategory")]
        public string QrCategory { get; set; }

        [JsonPropertyName("shortUrl")]
        public string ShortUrl { get; set; }

        [JsonPropertyName("redirectUrl")]
        public string RedirectUrl { get; set; }

        [JsonPropertyName("murlData")]
        public List<object> MurlData { get; set; }

        [JsonPropertyName("qrName")]
        public string QrName { get; set; }

        [JsonPropertyName("qrImage")]
        public string QrImage { get; set; }

        [JsonPropertyName("svgImage")]
        public string SvgImage { get; set; }

        [JsonPropertyName("owner")]
        public string Owner { get; set; }

        [JsonPropertyName("subUserId")]
        public object SubUserId { get; set; }

        [JsonPropertyName("qrConfig")]
        public string QrConfig { get; set; }

        [JsonPropertyName("scanData")]
        public List<object> ScanData { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("urlUpdatedAt")]
        public DateTime UrlUpdatedAt { get; set; }

        [JsonPropertyName("__v")]
        public int V { get; set; }

        [JsonPropertyName("fbProfileType")]
        public string FbProfileType { get; set; }
    }

    public class Lock
    {
        [JsonPropertyName("active")]
        public bool Active { get; set; }
    }

    public class WatchList
    {
        [JsonPropertyName("isWatching")]
        public bool IsWatching { get; set; }

        [JsonPropertyName("date")]
        public object Date { get; set; }
    }

    public class GpsTrackData
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("trackNearQr")]
        public bool TrackNearQr { get; set; }

        [JsonPropertyName("radius")]
        public int Radius { get; set; }
    }

    public class Country
    {
        [JsonPropertyName("_id")]
        public Id Id { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class City
    {
        [JsonPropertyName("_id")]
        public Id Id { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
    public class QrcodeScanData
    {
        public string DeviceType { get; set; }

        public string City { get; set; }

        public string CountryCode { get; set; }

        public int Scans { get; set; }

    }

    public class QrcodeScanDataResponse
    {
        public List<QrcodeScanData> List { get; set; }

        public int Total { get;set; }

    }
}
