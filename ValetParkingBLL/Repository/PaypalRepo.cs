using System;
using System.Text.Json;
using Braintree;
using Braintree.Exceptions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ValetParkingAPI.Models;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL.Models;
using ValetParkingDAL.Models.CustomerModels;
using ValetParkingDAL.Models.PaymentModels.cs;

namespace ValetParkingBLL.Repository
{
    public class PaypalRepo : IPaypal
    {
        private readonly IConfiguration _configuration;

        private readonly AppSettings _appsettings;

        private readonly IMaster _masterRepo;

        public PaypalRepo(IConfiguration configuration, IMaster masterRepo)
        {
            _configuration = configuration;
            _masterRepo = masterRepo;
            _appsettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
        }

        public (string, string) GetClientToken(PaypalModel model)
        {
            try
            {
                var gateway = new BraintreeGateway
                {
                    Environment = model.IsProduction ? Braintree.Environment.PRODUCTION : Braintree.Environment.SANDBOX,
                    MerchantId = model.MerchantId,
                    PublicKey = model.PublicKey,
                    PrivateKey = model.PrivateKey
                };
                var paypalCustomerId = string.Empty;

                if (string.IsNullOrEmpty(model.PaypalCustomerId))
                {
                    var request = new CustomerRequest
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email
                    };
                    //Create CustomerId
                    var result = gateway.Customer.Create(request);
                    paypalCustomerId = result.Target.Id;

                    //Update PaypalCustomerId
                    var paypal = new UpdatePaypalCustomerIdModel
                    {
                        CustomerId = model.CustomerId,
                        PaypalCustomerId = paypalCustomerId
                    };
                    _masterRepo.UpdatePaypalCustomerId(paypal);
                }
                else
                {
                    paypalCustomerId = model.PaypalCustomerId;
                }

                //Create Client Token
                var clientToken = gateway.ClientToken.Generate(
                    new ClientTokenRequest
                    {
                        CustomerId = paypalCustomerId
                    });
                return (paypalCustomerId, clientToken);

            }
            catch (Exception)
            {
                throw new AppException("Authorization Error");
            }
        }

        public dynamic ChargePayment(PaypalPaymentModel PaypalInfo, decimal Amount, CustomerPaymentDetails model)
        {

            var gateway = new BraintreeGateway
            {
                Environment = PaypalInfo.IsProduction ? Braintree.Environment.PRODUCTION : Braintree.Environment.SANDBOX,
                MerchantId = PaypalInfo.MerchantId,
                PublicKey = PaypalInfo.PublicKey,
                PrivateKey = PaypalInfo.PrivateKey
            };

            var request = new TransactionRequest
            {
                Amount = Amount,
                PaymentMethodNonce = PaypalInfo.Nonce,
                DeviceData = model.CustomerId.ToString(),
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                },
                CustomerId = model.PaypalCustomerId
            };
            try
            {
                var result = gateway.Transaction.Sale(request);
                var res = JsonConvert.SerializeObject(result);
                if (result.IsSuccess())
                {
                    return System.Text.Json.JsonSerializer.Deserialize<PaypalPaymentApiResponse>(res);
                }
                else
                {
                    return System.Text.Json.JsonSerializer.Deserialize<PaypalErrorResponse>(res);
                }
            }
            catch (Exception ex)
            {
                throw new AppException(ex.Message);
            }
        }
        public dynamic RefundPayment(CancelBookingDetails model)
        {
            var gateway = new BraintreeGateway
            {
                Environment = model.IsProduction ? Braintree.Environment.PRODUCTION : Braintree.Environment.SANDBOX,
                MerchantId = model.LocationId,
                PublicKey = model.ApiKey,
                PrivateKey = model.SecretKey
            };

            try
            {
                var result = gateway.Transaction.Refund(model.TransactionId, model.Amount);
                var res = JsonConvert.SerializeObject(result);
                if (result.IsSuccess())
                {
                    return System.Text.Json.JsonSerializer.Deserialize<PaypalRefundResponse>(res);
                }
                else
                {
                    return System.Text.Json.JsonSerializer.Deserialize<PaypalErrorResponse>(res);
                }
            }
            catch (Exception ex)
            {
                throw new AppException(ex.Message);
            }
        }


        public dynamic CancelPayment(CancelBookingDetails model)
        {
            var gateway = new BraintreeGateway
            {
                Environment = model.IsProduction ? Braintree.Environment.PRODUCTION : Braintree.Environment.SANDBOX,
                MerchantId = model.LocationId,
                PublicKey = model.ApiKey,
                PrivateKey = model.SecretKey
            };

            try
            {
                var result = gateway.Transaction.Void(model.TransactionId);
                var res = JsonConvert.SerializeObject(result);
                if (result.IsSuccess())
                {
                    return System.Text.Json.JsonSerializer.Deserialize<PaypalPaymentApiResponse>(res);
                }
                else
                {
                    return System.Text.Json.JsonSerializer.Deserialize<PaypalErrorResponse>(res);
                }
            }
            catch (Exception ex)
            {
                throw new AppException(ex.Message);
            }
        }

    }
}