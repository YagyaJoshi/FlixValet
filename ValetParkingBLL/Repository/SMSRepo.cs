using System;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using ValetParkingAPI.Models;
using ValetParkingBLL.Interfaces;
using ValetParkingDAL.Models;

namespace ValetParkingBLL.Repository
{
    public class SMSRepo : ISMS
    {
        private readonly IConfiguration _configuration;
        private readonly TwilioSettings _twilio;

        public SMSRepo(IConfiguration configuration)
        {
            _configuration = configuration;
            _twilio = _configuration.GetSection("twilio").Get<TwilioSettings>();
        }
        public void SendSMS(string Msg, string To, bool IsFromEnter=false)
        {
           
            try
            {
                To = To.Length.Equals(10) ? "+1" + To : To; // US Number
                To = To.StartsWith("+") ? To : "+" + To;

                TwilioClient.Init(
                             _twilio.AccountSid,
                              _twilio.AuthToken);

                MessageResource msg = MessageResource.Create(
                    to: new PhoneNumber(To),
                    from: new PhoneNumber(_twilio.SMSFrom),
                    body: Msg);
            }
            catch (Exception ex)
            {
                if(IsFromEnter) throw ex; else throw new AppException(ex.Message);
            }
            
        }
    }
}