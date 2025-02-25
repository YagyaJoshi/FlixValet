using System;
using System.Data.SqlClient;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RestSharp;
using Square;
using Square.Apis;
using Square.Exceptions;
using Square.Models;
using ValetParkingAPI.Models;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL;
using ValetParkingDAL.Enums;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingDAL.Models.PaymentModels.cs;

namespace ValetParkingBLL.Repository
{
    public class SquareRepo : ISquare
    {

        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly IEmail _emailService;
        private readonly AppSettings _appsettings;

        public SquareRepo(Microsoft.Extensions.Configuration.IConfiguration configuration, IEmail emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
            _appsettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
        }

        public dynamic ChargePayment(SquareupModel SquareupInfo, decimal Amount, CustomerPaymentDetails model)
        {

            string RandomGuid, idempotency_key;

            RandomGuid = Guid.NewGuid().ToString();
            idempotency_key = RandomGuid.Substring(0, RandomGuid.LastIndexOf('-')) + '_' + DateTime.Now.ToString("yyyyMMddHHmmss");
            var RequestObject = new SquareupPaymentRequest
            {
                source_id = SquareupInfo.SourceId,
                idempotency_key = idempotency_key,
                amount_money = new AmountModel
                {
                    amount = Convert.ToInt64(Amount * 100),
                    currency = model.Currency
                },
                billing_address = new BillingAddress
                {
                    address_line_1 = model.Address,
                    administrative_district_level_1 = model.State,
                    administrative_district_level_2 = model.Country,
                    first_name = model.CustomerName.Split(' ')[0],
                    last_name = model.CustomerName.Split(' ')[1],
                    locality = model.City,
                    postal_code = model.ZipCode
                }
            };


            var json = JsonSerializer.Serialize(RequestObject);

            var client = new RestClient($"{(SquareupInfo.IsProduction ? _appsettings.SquareupProductionUrl : _appsettings.SquareupBaseUrl)}v2/payments");
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("Authorization", $"Bearer {SquareupInfo.AccessToken}");
            request.AddHeader("Accept", "application/json");
            //request.Parameters.Clear();
            request.AddParameter("application/json", json, ParameterType.RequestBody);

            var response = client.Execute(request);

            if (response.StatusCode.Equals(HttpStatusCode.OK))
            {
                var apiresponse = JsonSerializer.Deserialize<SquareupChargesApiResponse>(response.Content);
                apiresponse.payment.idempotency_key = idempotency_key;
                return apiresponse;
            }
            else
            {

                var errorResp = JsonSerializer.Deserialize<SquareupErrorResponse>(response.Content);

                if (response.StatusCode.Equals(HttpStatusCode.Unauthorized) && errorResp.errors != null)
                {
                    errorResp.errors[0].detail = "The Merchant is unauthorized for taking payments. Try for On-site booking.";

                    _emailService.WDLogError("Book Parking Location", SquareupInfo.BusinessTitle + " owner has provided invalid payment credentials for Square up payment.");
                };

                return errorResp;
            }

        }

        public dynamic RefundPayment(CancelBookingDetails model)
        {
            string RandomGuid, idempotency_key;

            SquareClient square = new SquareClient.Builder()
               .Environment(!model.IsProduction ? Square.Environment.Sandbox : Square.Environment.Production)
               .AccessToken(model.AccessToken)
               .Build();

            RandomGuid = Guid.NewGuid().ToString();
            idempotency_key = RandomGuid.Substring(0, RandomGuid.LastIndexOf('-')) + '_' + DateTime.Now.ToString("yyyyMMddHHmmss");
            var amountMoney = new Money.Builder()
                            .Amount(Convert.ToInt64(model.Amount * 100))
                            .Currency(model.Currency)
                            .Build();

            var body = new RefundPaymentRequest.Builder(
                idempotencyKey: idempotency_key,
                amountMoney: amountMoney,
                paymentId: model.TransactionId)
              .Build();

            try
            {
                RefundPaymentResponse response = square.RefundsApi.RefundPayment(body: body);

                if (response.Refund.Status.Equals(ESquareRefundStatus.PENDING.ToString()))
                {
                    try
                    {
                        var result = square.RefundsApi.GetPaymentRefund(refundId: response.Refund.Id);
                        return result.Refund;
                    }
                    catch (ApiException e)
                    {
                        return e.Errors[0].Detail;
                    }
                }
                return response.Refund;
            }
            catch (ApiException e)
            {
                return e.Errors[0].Detail;

            }
        }

        public async Task<dynamic> GetSquareUpReaderToken(SquareUpRequest model)
        {
            var body = new CreateMobileAuthorizationCodeRequest.Builder()
            .LocationId(model.LocationId)
            .Build();

            SquareClient client = new SquareClient.Builder()
             .AccessToken(model.AccessToken)
             .Build();
            try
            {
                var result = await client.MobileAuthorizationApi.CreateMobileAuthorizationCodeAsync(body);
                return result;
            }
            catch (ApiException ex)
            {
                throw new AppException(ex.Errors[0].Detail);
            }

        }
    }
}