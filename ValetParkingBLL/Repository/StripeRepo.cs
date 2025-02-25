using System.Linq;
using ValetParkingDAL.Models;
using System;
using System.Data;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RestSharp;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingDAL.Models.PaymentModels.cs;
using System.Net;
using System.Data.SqlClient;
using Stripe;

namespace ValetParkingBLL.Repository
{
    public class StripeRepo : IStripe
    {
        private readonly IConfiguration _configuration;

        private readonly AppSettings _appsettings;

        public StripeRepo(IConfiguration configuration)
        {
            _configuration = configuration;
            _appsettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
        }

        public dynamic ChargePayment(StripeModel StripeInfo, decimal Amount, CustomerPaymentDetails model)
        {

            var client = new RestClient($"{_appsettings.StripeBaseUrl}v1/charges");
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("Authorization", $"Bearer {StripeInfo.SecretKey}");
            request.AddParameter("application/x-www-form-urlencoded", $"amount={(int)(Amount * 100)}&currency={model.Currency}&source={StripeInfo.Token}&shipping[name]={model.CustomerName}&shipping[address][line1]={model.Address}&shipping[address][postal_code]={model.ZipCode}&shipping[address][city]={model.City}&shipping[address][state]={model.State}&shipping[address][country]={model.Country}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonSerializer.Deserialize<StripeChargesApiResponse>(response.Content.Replace("\n", ""));
            }
            else
            {
                return JsonSerializer.Deserialize<StripeErrorResponse>(response.Content.Replace("\n", ""));

            }
        }

        public StripeModel GetCredentials(long ParkingLocationId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetStripeCredentials");
            try
            {
                objCmd.Parameters.AddWithValue("@ParkingLocationId", ParkingLocationId);
                DataTable dtInfo = objSQL.FetchDT(objCmd);

                var Info = (from DataRow dr in dtInfo.Rows
                            select new StripeModel
                            {
                                ApiKey = dr["ApiKey"] == DBNull.Value ? null : Convert.ToString(dr["ApiKey"]),
                                SecretKey = dr["SecretKey"] == DBNull.Value ? null : Convert.ToString(dr["SecretKey"])

                            }).FirstOrDefault();
                return Info;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public dynamic RefundPayment(CancelBookingDetails model)
        {

            StripeConfiguration.ApiKey = model.SecretKey;

            var options = new RefundCreateOptions
            {
                Charge = model.TransactionId,
            };
            var service = new RefundService();
            try
            {
                var refund = service.Create(options);
                return refund;
            }
            catch (StripeException ex)
            {
                return ex.StripeError.Message;
            }

        }
    }
}