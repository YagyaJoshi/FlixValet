using ValetParkingBLL.Interfaces;
using ValetParkingDAL.Models;
using System;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;

namespace ValetParkingBLL.Repository
{
    public class EmailRepo : IEmail
    {
        private readonly AppSettings _appSettings;
        private readonly IConfiguration _configuration;
        private readonly TwilioSettings _twilio;

        public EmailRepo(IConfiguration configuration)
        {
            _configuration = configuration;
            _appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
            _twilio = _configuration.GetSection("twilio").Get<TwilioSettings>();
        }

        public async Task Send(string to, string subject, string html)
        {
            var apikey = _twilio.ApiKey;

            var client = new SendGridClient(apikey);
            var from = new EmailAddress(_twilio.EmailFrom, _appSettings.AppName);
            var toAdrress = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toAdrress, subject, "", html);
            var resp = await client.SendEmailAsync(msg);


            #region old mail code.
            // create message
            // var email = new MimeMessage();
            // email.From.Add(new MailboxAddress
            //                             (_appSettings.EmailFrom,
            //                             _appSettings.EmailFrom
            //                              ));

            // email.To.Add(MailboxAddress.Parse(to));
            // email.Subject = subject;
            // email.Body = new TextPart(TextFormat.Html) { Text = html };

            // // send email
            // var smtp = new SmtpClient();
            // // smtp.Connect(_appSettings.SmtpHost, _appSettings.SmtpPort, SecureSocketOptions.StartTls);
            // smtp.Connect(_appSettings.SmtpHost, _appSettings.SmtpPort, SecureSocketOptions.StartTls);
            // smtp.Authenticate(_appSettings.SmtpUser, _appSettings.SmtpPass);
            // smtp.Send(email);
            // smtp.Disconnect(true);
            #endregion

        }

        public async Task SendPaymentErrorEmail()
        {
            var apikey = _twilio.ApiKey;
            var client = new SendGridClient(apikey);
            var from = new EmailAddress(_twilio.EmailFrom, _appSettings.AppName);
            var to = new EmailAddress("mahimakhore.synsoft@gmail.com", "Mahima");
            var Content = "<b>test by mahima</b>";
            var msg = MailHelper.CreateSingleEmail(from, to, "FlixValet", "", Content);
            var resp = await client.SendEmailAsync(msg);
        }

        public async Task WDLogError(string p_URL, string p_Error)
        {
            var apikey = _twilio.ApiKey;
            var client = new SendGridClient(apikey);
            var from = new EmailAddress(_twilio.EmailFrom, _appSettings.AppName);
            var subject = "Flix Valet App - Error Occurred";
            List<EmailAddress> Recipients = new List<EmailAddress>();
            Recipients.Add(new EmailAddress { Email = "mahimakhore.synsoft@gmail.com" });
            string body = string.Format("<h2>Flix Valet App Error Log</h2> Error Details: <br/> " + p_Error + "<br/><br/> URL:  " + p_URL + " <br/> Error Date & Time: {0}", DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss tt"));

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, Recipients, subject, "", body);
            var resp = await client.SendEmailAsync(msg);

            // var email = new MimeMessage();
            // email.From.Add(new MailboxAddress
            //                             (_appSettings.EmailFrom,
            //                             _appSettings.EmailFrom
            //                              ));
            // email.To.Add(MailboxAddress.Parse("naina.synsoft@gmail.com"));
            // email.To.Add(MailboxAddress.Parse("mahimakhore.synsoft@gmail.com"));
            // email.Subject = "Flix Valet App - Error Occurred";

            // string l_Body = string.Format("<h2>Flix Valet App Error Log</h2> Error Details: <br/> " + p_Error + "<br/><br/> URL:  " + p_URL + " <br/> Error Date & Time: {0}", DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss tt"));

            // email.Body = new TextPart(TextFormat.Html) { Text = l_Body };

            // // send email
            // var smtp = new SmtpClient();
            // // smtp.Connect(_appSettings.SmtpHost, _appSettings.SmtpPort, SecureSocketOptions.StartTls);
            // smtp.Connect(_appSettings.SmtpHost, _appSettings.SmtpPort, SecureSocketOptions.StartTls);
            // smtp.Authenticate(_appSettings.SmtpUser, _appSettings.SmtpPass);
            // smtp.Send(email);
            // smtp.Disconnect(true);
        }
    }
}