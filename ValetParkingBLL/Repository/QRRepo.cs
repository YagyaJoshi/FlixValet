using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RestSharp;
using ValetParkingAPI.Models;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.QRModels;

namespace ValetParkingBLL.Repository
{
    public class QRRepo : IQR
    {
        private readonly IConfiguration _configuration;
        private readonly TigerQRSettings _tigerqrSettings;
        private readonly AppSettings _appsettings;
        private readonly ICache _cacheRepo;

        public QRRepo(IConfiguration configuration, ICache cacheRepo)
        {
            _configuration = configuration;
            _tigerqrSettings = _configuration.GetSection("TigerQR").Get<TigerQRSettings>();
            _appsettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
            _cacheRepo = cacheRepo;
        }
        public string GetDynamicTigerQRImage(string Text, string LogoUrl)
        {
            try
            {
                var RequestBody = new TigerQRRequest
                {
                    qr = new Qr
                    {
                        size = 500,
                        colorDark = "#000000",
                        gradient = false,
                        grdType = "diagonal1",
                        logo = LogoUrl,
                        eye_outer = "eyeOuter2",
                        eye_inner = "eyeInner2",
                        qrFormat = "png",
                        qrData = "pattern3",
                        backgroundColor = "rgb(255,255,255)",
                        color01 = "#000000",
                        color02 = "#000000",
                        transparentBkg = false,
                        frame = 1,
                        frameColor = "#054080",
                        frameText = "SCAN ME"

                    },
                    qrUrl = Text,
                    qrType = "qr2",
                    qrCategory = "url"
                };
                var json = JsonSerializer.Serialize(RequestBody);
                var client = new RestClient($"{_tigerqrSettings.BaseUrl}api/campaign");
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("Authorization", $"Bearer {_tigerqrSettings.ApiKey}");
                request.AddHeader("Accept", "application/json");
                request.AddParameter("application/json", json, ParameterType.RequestBody);

                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var tgresponse = JsonSerializer.Deserialize<TigerQRResponse>(response.Content);
                    return tgresponse.imageUrl;
                }
                else return null;
            }
            catch (Exception ex)
            {
                throw new AppException(ex.Message);
            }
        }

        public string GetStaticTigerQRImage(string Text)
        {
            try
            {
                var RequestBody = new StaticTigerQRRequest
                {
                    size = 500,
                    colorDark = "#000000",
                    gradient = false,
                    grdType = "diagonal1",
                    logo = _appsettings.LogoUrl,
                    eye_outer = "eyeOuter2",
                    eye_inner = "eyeInner2",
                    qrFormat = "png",
                    qrData = "pattern3",
                    backgroundColor = "rgb(255,255,255)",
                    color01 = "#000000",
                    color02 = "#000000",
                    transparentBkg = false,
                    frame = 1,
                    frameColor = "#054080",
                    qrCategory = "text", //url
                    text = Text,
                    frameText = "SCAN ME"
                };

                var json = JsonSerializer.Serialize(RequestBody);
                var client = new RestClient($"{_tigerqrSettings.BaseUrl}api/qr/static");
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("Authorization", $"Bearer {_tigerqrSettings.ApiKey}");
                request.AddHeader("Accept", "application/json");
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var tgresponse = JsonSerializer.Deserialize<StaticTigerQRResponse>(response.Content);


                    return tgresponse.url;
                }
                else return null;
            }
            catch (Exception ex)
            {
                throw new AppException(ex.Message);
            }
        }

        public dynamic GetCompressedStaticTigerQRImage(string Text, string LogoUrl)
        {
            try
            {
                var RequestBody = new StaticTigerQRRequest
                {
                    size = 200,
                    colorDark = "#000000",
                    gradient = false,
                    grdType = "diagonal1",
                    // logo = _appsettings.LogoUrl,
                    logo = LogoUrl,
                    eye_outer = "eyeOuter2",
                    eye_inner = "eyeInner2",
                    qrFormat = "png",
                    qrData = "pattern3",
                    backgroundColor = "rgb(255,255,255)",
                    color01 = "#000000",
                    color02 = "#000000",
                    transparentBkg = false,
                    frame = 1,
                    frameColor = "#054080",
                    qrCategory = "text", //url
                    text = Text,
                    frameText = "SCAN ME"
                };

                var json = JsonSerializer.Serialize(RequestBody);
                var client = new RestClient($"{_tigerqrSettings.BaseUrl}api/qr/static");
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("Authorization", $"Bearer {_tigerqrSettings.ApiKey}");
                request.AddHeader("Accept", "application/json");
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var tgresponse = JsonSerializer.Deserialize<StaticTigerQRResponse>(response.Content);


                    return tgresponse;
                }
                else return null;
            }
            catch (Exception ex)
            {
                throw new AppException(ex.Message);
            }
        }

        public QRCodeDataResponse GetQrCodeScanData(string qrId, string timeZone, DateTime date)
        {
            try
            {
                var timeZones = _cacheRepo.CachedTimeZones();
                var tz = timeZones.Where(e => e.TimeZoneId == timeZone).Select(a => a.Name).FirstOrDefault();
                // Convert local time (Asia/Calcutta) to UTC
                TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(tz);
                DateTime utcDate = TimeZoneInfo.ConvertTimeToUtc(date, tzInfo);

                // Convert UTC DateTime to Unix timestamp (in seconds)
                long unixTimestamp = ((DateTimeOffset)utcDate).ToUnixTimeSeconds();
                var client = new RestClient($"https://api.qrtiger.com/api/data/{qrId}?period=day&tz={timeZone}&timestamp={unixTimestamp}");
                var request = new RestRequest(Method.GET);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("Authorization", $"Bearer {_tigerqrSettings.ApiKey}");
                request.AddHeader("Accept", "application/json");

                IRestResponse response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var tigerQRResponse = JsonSerializer.Deserialize<QRCodeDataResponse>(response.Content);
                    return tigerQRResponse;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new AppException(ex.Message);
            }
        }
    }
}