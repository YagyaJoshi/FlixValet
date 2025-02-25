using System;
using System.IO;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RestSharp;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.NumberPlateRecogModels;

namespace ValetParkingBLL.Repository
{
    public class NumberPlateRecognitionRepo : INumberPlateRecognition
    {
        private readonly IConfiguration _configuration;

        private readonly RekorScoutSettings _rekorScoutSettings;

        public NumberPlateRecognitionRepo(IConfiguration configuration)
        {
            _configuration = configuration;

            _rekorScoutSettings = _configuration.GetSection("RekorScout").Get<RekorScoutSettings>();
        }
        public dynamic GetNumberPlateFromImg(string ImagePath)
        {
            var client = new RestClient($"{_rekorScoutSettings.BaseUrl}v3/recognize_bytes?recognize_vehicle=1&country=us&secret_key={_rekorScoutSettings.SecretKey}");

            //var webClient = new WebClient();
            // byte[] bytes = webClient.DownloadData(ImagePath);
            Byte[] bytes = File.ReadAllBytes(ImagePath);
            string imagebase64 = Convert.ToBase64String(bytes);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "text/plain");
            request.AddParameter("text/plain", imagebase64, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonSerializer.Deserialize<NumberPlateApiResponse>(response.Content);
            }
            else
            {
                return JsonSerializer.Deserialize<NumberPlateErrorResponse>(response.Content);

            }
        }
    }
}